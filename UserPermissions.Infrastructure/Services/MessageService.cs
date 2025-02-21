using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using UserPermissions.Application.Services;

namespace UserPermissions.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public MessageService(IProducer<string, string> producer, IConfiguration configuration)
        {
            _producer = producer;
            _topic = configuration["Kafka:TopicName"];
        }

        public async Task PublishAsync(string message, CancellationToken cancellationToken)
        {
            var kafkaMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            };
            await _producer.ProduceAsync(_topic, kafkaMessage, cancellationToken);
        }
    }
}
