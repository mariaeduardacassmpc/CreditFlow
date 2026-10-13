using System.Text.Json;
using Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Messaging;

public sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers não configurado.");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(topic,
            new Message<Null, string>
            {
                Value = json
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
