using Confluent.Kafka;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Domain.Events;

namespace Infrastructure.Messaging;

public class KafkaConsumer(
    IServiceScopeFactory scopeFactory, 
    ILogger<KafkaConsumer> logger
) : BackgroundService
{
    private readonly ConsumerConfig config = new()
    {
        BootstrapServers = "localhost:29092",
        GroupId = "creditflow-analysis",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe("credit-request-created");

        logger.LogInformation("Kafka Consumer iniciado. Aguardando solicitações...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                logger.LogInformation("Evento recebido: {Message}", result.Message.Value);

                var eventData = JsonSerializer.Deserialize<CreditRequestCreatedEvent>(result.Message.Value);

                if (eventData is null)
                    continue;

                using var scope = scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<CreditFlowDbContext>();

                var creditAnalysisService = scope.ServiceProvider
                    .GetRequiredService<CreditAnalysisService>();

                var creditRequest = await context.CreditRequests
                    .Include(x => x.Customer)
                    .FirstOrDefaultAsync(x => x.CreditRequestId == eventData.CreditRequestId, stoppingToken);

                if (creditRequest is null)
                {
                    logger.LogWarning("Solicitação {CreditRequestId} não encontrada.", 
                        eventData.CreditRequestId);
                    continue;
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

                await context.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Análise da solicitação {CreditRequestId} concluída. Status: {Status}",
                    creditRequest.CreditRequestId, analysisResult.Status);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Kafka Consumer encerrado.");
        }
        finally
        {
            consumer.Close();
        }
    }
}