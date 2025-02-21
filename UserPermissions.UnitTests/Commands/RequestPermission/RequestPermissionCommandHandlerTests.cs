using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Commands.RequestPermission;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.Services;
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
        private readonly Mock<IMessageService> _messageServiceMock;
        private readonly Mock<IElasticClient> _elasticClientMock;
        private readonly RequestPermissionCommandHandler _handler;

        public RequestPermissionCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _messageServiceMock = new Mock<IMessageService>();
            _elasticClientMock = new Mock<IElasticClient>();

            _handler = new RequestPermissionCommandHandler(
                _unitOfWorkMock.Object,
                _messageServiceMock.Object,
                _elasticClientMock.Object);
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
            _messageServiceMock.Setup(ms => ms.PublishAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _elasticClientMock.Setup(client => client.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Nest.IndexResponse());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_ValidRequest_AddsPermission()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Id = 1, Name = "Test Permission Type" };
            var command = new RequestPermissionCommand
            {
                EmployeeId = employee.Id,
                PermissionTypeId = permissionType.Id,
                Description = "New Permission",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };

            _unitOfWorkMock.Setup(uow => uow.EmployeeReadRepository.GetEmployeeByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(employee);
            _unitOfWorkMock.Setup(uow => uow.PermissionTypeReadRepository.GetPermissionTypeByIdAsync(permissionType.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissionType);
            _unitOfWorkMock.Setup(uow => uow.PermissionRepository.AddPermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _elasticClientMock.Setup(ec => ec.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndexResponse());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(uow => uow.PermissionRepository.AddPermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
            _messageServiceMock.Verify(ms => ms.PublishAsync("Request", It.IsAny<CancellationToken>()), Times.Once);
            _elasticClientMock.Verify(ec => ec.IndexDocumentAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
