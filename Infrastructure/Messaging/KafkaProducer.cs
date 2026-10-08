using System.Text.Json;
using Confluent.Kafka;

namespace Infrastructure.Messaging;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, string> producer;

    public KafkaProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:29092"
        };

        producer = new ProducerBuilder<Null, string>(config)
            .Build();
    }

    public async Task PublishAsync<T>(string topic, T message)
    {
        var json = JsonSerializer.Serialize(message);
        
        var result = await producer.ProduceAsync(topic,
              new Message<Null, string>
              {
                  Value = json
              }
        );
    }
}