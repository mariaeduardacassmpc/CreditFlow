using Application.Interfaces;
using Confluent.Kafka;
using Domain.Entities;
using Domain.Events;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Messaging;

public class KafkaConsumer(IServiceScopeFactory scopeFactory, IKafkaProducer kafkaProducer, ILogger<KafkaConsumer> logger) : BackgroundService
{
    private const string Topic = "credit-request-created";
    private const string DeadLetterTopic = "credit-request-created-dlq";
    private const int MaxAttempts = 3;

    private readonly ConsumerConfig config = new()
    {
        BootstrapServers = "localhost:29092",
        GroupId = "creditflow-analysis",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        EnableAutoOffsetStore = false
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(Topic);

        logger.LogInformation("Kafka Consumer iniciado. Tópico: {Topic}", Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> result;

                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Erro ao consumir mensagem do Kafka.");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                var processed = false;
                Exception? lastException = null;

                for (var attempt = 1; attempt <= MaxAttempts && !processed; attempt++)
                {
                    try
                    {
                        await ProcessMessageAsync(result.Message.Value, stoppingToken);
                        processed = true;
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;

                        logger.LogWarning(ex, "Falha ao processar mensagem. Tentativa {Attempt}/{MaxAttempts}. Partição: {Partition}, Offset: {Offset}", attempt, MaxAttempts, result.Partition.Value, result.Offset.Value);

                        if (attempt < MaxAttempts)
                            await Task.Delay(TimeSpan.FromSeconds(attempt * 2), stoppingToken);
                    }
                }

                if (processed)
                {
                    consumer.Commit(result);
                    logger.LogInformation("Mensagem processada e offset confirmado. Partição: {Partition}, Offset: {Offset}", result.Partition.Value, result.Offset.Value);
                    continue;
                }

                try
                {
                    var deadLetterMessage = new
                    {
                        OriginalTopic = result.Topic,
                        Partition = result.Partition.Value,
                        Offset = result.Offset.Value,
                        OriginalMessage = result.Message.Value,
                        Error = lastException?.Message ?? "Erro desconhecido",
                        ErrorType = lastException?.GetType().FullName,
                        FailedAtUtc = DateTime.UtcNow
                    };

                    await kafkaProducer.PublishAsync(DeadLetterTopic, deadLetterMessage, stoppingToken);
                    consumer.Commit(result);

                    logger.LogError(lastException, "Mensagem encaminhada à DLQ {DeadLetterTopic}. Offset original confirmado: {Offset}", DeadLetterTopic, result.Offset.Value);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Falha ao publicar mensagem na DLQ. O offset original não será confirmado.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    consumer.Seek(result.TopicPartitionOffset);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Kafka Consumer encerrado.");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        var eventData = JsonSerializer.Deserialize<CreditRequestCreatedEvent>(message);

        if (eventData is null || eventData.CreditRequestId <= 0)
            throw new JsonException("Evento de solicitação de crédito inválido.");

        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CreditFlowDbContext>();
        var creditAnalysisService = scope.ServiceProvider.GetRequiredService<CreditAnalysisService>();

        var creditRequest = await context.CreditRequests
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.CreditRequestId == eventData.CreditRequestId, cancellationToken);

        if (creditRequest is null)
            throw new InvalidOperationException($"Solicitação {eventData.CreditRequestId} não encontrada.");

        var alreadyAnalyzed = await context.CreditAnalyses
            .AnyAsync(x => x.CreditRequestId == creditRequest.CreditRequestId, cancellationToken);

        if (alreadyAnalyzed)
        {
            logger.LogInformation("Solicitação {CreditRequestId} já analisada. Ignorando evento duplicado.", creditRequest.CreditRequestId);
            return;
        }

        var analysisResult = creditAnalysisService.Analyze(creditRequest);

        var analysis = new CreditAnalysis
        {
            CreditRequestId = creditRequest.CreditRequestId,
            Status = analysisResult.Status,
            AnalyzedAt = DateTime.UtcNow
        };

        foreach (var rule in analysisResult.Rules)
        {
            analysis.Rules.Add(new CreditRuleResult
            {
                Description = rule.Description,
                Approved = rule.Approved
            });
        }

        creditRequest.Status = analysisResult.Status;
        context.CreditAnalyses.Add(analysis);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Análise da solicitação {CreditRequestId} concluída. Status: {Status}", creditRequest.CreditRequestId, analysisResult.Status);
    }
}