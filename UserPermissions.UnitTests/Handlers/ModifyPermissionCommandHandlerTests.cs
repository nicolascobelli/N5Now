using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Commands.ModifyPermission;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using Xunit;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Nest;

namespace UserPermissions.UnitTests.Handlers
{
    public class ModifyPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProducer<string, string>> _producerMock;
        private readonly Mock<IElasticClient> _elasticClientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ModifyPermissionCommandHandler _handler;

        public ModifyPermissionCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _producerMock = new Mock<IProducer<string, string>>();
            _elasticClientMock = new Mock<IElasticClient>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Kafka:Topic"]).Returns("test-topic");

            _handler = new ModifyPermissionCommandHandler(
                _unitOfWorkMock.Object,
                _producerMock.Object,
                _elasticClientMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ReturnsFalse()
        {
            // Arrange
            var command = new ModifyPermissionCommand { EmployeeId = 1, PermissionId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Employee)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_PermissionNotFound_ReturnsFalse()
        {
            // Arrange
            var command = new ModifyPermissionCommand { EmployeeId = 1, PermissionId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Employee { Name = "Test Employee", Email = "test@example.com" });
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.GetPermissionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Permission)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var employee = new Employee { Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Name = "Test Permission Type" };
            var permission = new Permission("Existing Permission", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));

            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(employee);
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.GetPermissionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(permission);
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.UpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _producerMock.Setup(producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DeliveryResult<string, string>());
            _elasticClientMock.Setup(client => client.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Nest.IndexResponse());

            var command = new ModifyPermissionCommand
            {
                EmployeeId = employee.Id,
                PermissionId = permission.Id,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(3)
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }
    }
}
