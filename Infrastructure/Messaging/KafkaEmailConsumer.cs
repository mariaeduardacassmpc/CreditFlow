using Application.Interfaces;
using Confluent.Kafka;
using Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Messaging;

public class KafkaEmailConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<KafkaEmailConsumer> logger
) : BackgroundService
{
    private readonly ConsumerConfig config = new()
    {
        BootstrapServers = "localhost:29092",
        GroupId = "creditflow-email",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        EnableAutoOffsetStore = false
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(new[]
        {
            "two-factor-code-requested",
            "password-reset-requested"
        });

        logger.LogInformation("Consumidor de e-mails iniciado.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                try
                {
                    using var scope = scopeFactory.CreateScope();

                    var emailService = scope.ServiceProvider
                        .GetRequiredService<IEmailService>();

                    switch (result.Topic)
                    {
                        case "two-factor-code-requested":
                            {
                                var emailEvent =
                                    JsonSerializer.Deserialize
                                        <TwoFactorCodeRequestedEvent>(
                                            result.Message.Value);

                                if (emailEvent is null ||
                                    string.IsNullOrWhiteSpace(emailEvent.Email) ||
                                    string.IsNullOrWhiteSpace(emailEvent.Code))
                                {
                                    logger.LogWarning(
                                        "Evento 2FA inválido.");
                                    break;
                                }

                                await emailService.SendTwoFactorCodeEmail(
                                    emailEvent.Email,
                                    emailEvent.Code);

                                logger.LogInformation(
                                    "E-mail 2FA processado.");
                                break;
                            }

                        case "password-reset-requested":
                            {
                                var emailEvent =
                                    JsonSerializer.Deserialize
                                        <PasswordResetRequestedEvent>(
                                            result.Message.Value);

                                if (emailEvent is null ||
                                    string.IsNullOrWhiteSpace(emailEvent.Email) ||
                                    string.IsNullOrWhiteSpace(emailEvent.Code))
                                {
                                    logger.LogWarning(
                                        "Evento de recuperação inválido.");
                                    break;
                                }

                                await emailService.SendPasswordResetEmail(
                                    emailEvent.Email,
                                    emailEvent.Code);

                                logger.LogInformation(
                                    "E-mail de recuperação processado.");
                                break;
                            }
                    }
                    consumer.Commit(result);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Falha ao processar mensagem do tópico {Topic}.",
                        result.Topic);

                    consumer.Seek(result.TopicPartitionOffset);

                    await Task.Delay(
                        TimeSpan.FromSeconds(3),
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Consumidor de e-mails encerrado.");
        }
        finally
        {
            consumer.Close();
        }
    }
}