namespace Application.Interfaces;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message);
}