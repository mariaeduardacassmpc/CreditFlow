namespace Infrastructure.Messaging;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message);
}