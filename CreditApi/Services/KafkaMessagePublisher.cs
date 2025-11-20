using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace CreditApi.Services
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string topic, string message);
    }

    public class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaMessagePublisher> _logger;

        public KafkaMessagePublisher(
            IProducer<Null, string> producer, 
            ILogger<KafkaMessagePublisher> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        public async Task PublishAsync(string topic, string message)
        {
            try
            {
                var deliveryResult = await _producer.ProduceAsync(
                    topic,
                    new Message<Null, string> { Value = message }
                );

                _logger.LogInformation(
                    "Mensagem enviada para o tópico {Topic} | Partição {Partition} | Offset {Offset}",
                    deliveryResult.Topic, 
                    deliveryResult.Partition, 
                    deliveryResult.Offset
                );
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(
                    ex, 
                    "Erro ao enviar mensagem para o Kafka: {Reason}", 
                    ex.Error.Reason
                );

                // Boa prática: tenta flushar antes de falhar
                _producer.Flush(TimeSpan.FromSeconds(5));

                throw; // relança para tratamento global
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao publicar mensagem no Kafka");
                throw;
            }
        }
    }
}
