using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Commands.RequestPermission;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using Xunit;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Nest;

namespace UserPermissions.UnitTests.Handlers
{
    public class RequestPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProducer<string, string>> _producerMock;
        private readonly Mock<IElasticClient> _elasticClientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly RequestPermissionCommandHandler _handler;

        public RequestPermissionCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _producerMock = new Mock<IProducer<string, string>>();
            _elasticClientMock = new Mock<IElasticClient>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Kafka:Topic"]).Returns("test-topic");

            _handler = new RequestPermissionCommandHandler(
                _unitOfWorkMock.Object,
                _producerMock.Object,
                _elasticClientMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ReturnsNull()
        {
            // Arrange
            var command = new RequestPermissionCommand { EmployeeId = 1, PermissionTypeId = 1, Description = "Test Description", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Employee)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_PermissionTypeNotFound_ReturnsNull()
        {
            // Arrange
            var command = new RequestPermissionCommand { EmployeeId = 1, PermissionTypeId = 1, Description = "Test Description", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Employee { Name = "Test Employee", Email = "test@example.com" });
            _unitOfWorkMock.Setup(uow => uow.PermissionTypeReadRepository.GetPermissionTypeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((PermissionType)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsPermissionId()
        {
            // Arrange
            var command = new RequestPermissionCommand { EmployeeId = 1, PermissionTypeId = 1, Description = "Test Description", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
            var permissionType = new PermissionType { Name = "Test Permission Type" };
            var permission = new Permission(command.Description, command.EmployeeId, command.PermissionTypeId, command.StartDate, command.EndDate);

            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Employee { Name = "Test Employee", Email = "test@example.com" });
            _unitOfWorkMock.Setup(uow => uow.PermissionTypeReadRepository.GetPermissionTypeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(permissionType);
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.AddPermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _producerMock.Setup(producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DeliveryResult<string, string>());
            _elasticClientMock.Setup(client => client.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Nest.IndexResponse());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}
