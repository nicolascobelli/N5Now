using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Moq;
using UserPermissions.Application.Services;
using UserPermissions.Infrastructure.Services;
using Xunit;

namespace UserPermissions.UnitTests.Services
{
    public class MessageServiceTests
    {
        private readonly Mock<IProducer<string, string>> _producerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly IMessageService _messageService;

        public MessageServiceTests()
        {
            _producerMock = new Mock<IProducer<string, string>>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Kafka:TopicName"]).Returns("test-topic");

            _messageService = new MessageService(_producerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task PublishAsync_ValidMessage_PublishesToKafka()
        {
            // Arrange
            var message = "Test message";
            var cancellationToken = CancellationToken.None;

            _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<string, string>());

            // Act
            await _messageService.PublishAsync(message, cancellationToken);

            // Assert
            _producerMock.Verify(p => p.ProduceAsync("test-topic", It.Is<Message<string, string>>(m => m.Value == message), cancellationToken), Times.Once);
        }
    }
}
