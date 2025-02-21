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
using UserPermissions.Application.Services;

namespace UserPermissions.UnitTests.Handlers
{
    public class ModifyPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IElasticClient> _elasticClientMock;
        private readonly Mock<IMessageService> _messageServiceMock;
        private readonly ModifyPermissionCommandHandler _handler;

        public ModifyPermissionCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _elasticClientMock = new Mock<IElasticClient>();
            _messageServiceMock = new Mock<IMessageService>();


            _handler = new ModifyPermissionCommandHandler(
                _unitOfWorkMock.Object,
                _messageServiceMock.Object,
                _elasticClientMock.Object);
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

        [Fact]
        public async Task Handle_ValidRequest_UpdatesPermission()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Id = 1, Name = "Test Permission Type" };
            var permission = new Permission("Existing Permission", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));
            var command = new ModifyPermissionCommand
            {
                EmployeeId = employee.Id,
                PermissionId = permission.Id,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(3)
            };

            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(employee);
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.GetPermissionByIdAsync(permission.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permission);
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.UpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _elasticClientMock.Setup(ec => ec.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndexResponse());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(uow => uow.PermissionRepository.UpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
            _messageServiceMock.Verify(ms => ms.PublishAsync("Modify", It.IsAny<CancellationToken>()), Times.Once);
            _elasticClientMock.Verify(ec => ec.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
