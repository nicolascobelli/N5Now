using System;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Queries.GetPermissions;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.DTOs;
using Xunit;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using UserPermissions.Application.Services;

namespace UserPermissions.UnitTests.Handlers
{
    public class GetPermissionsQueryHandlerTests
    {
        private readonly Mock<IPermissionsReadRepository> _permissionRepositoryMock;
        private readonly GetPermissionsQueryHandler _handler;
        private readonly Mock<IMessageService> _messageServiceMock;

        public GetPermissionsQueryHandlerTests()
        {
            _permissionRepositoryMock = new Mock<IPermissionsReadRepository>();
            _messageServiceMock = new Mock<IMessageService>();

            _handler = new GetPermissionsQueryHandler(
                _permissionRepositoryMock.Object,
                _messageServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsPermissionsByEmployee()
        {
            // Arrange
            var employeeId = 1;
            var permissions = new List<PermissionDto>
            {
                new PermissionDto { Id = 1, Description = "Permission 1", Type = "Type 1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) },
                new PermissionDto { Id = 2, Description = "Permission 2", Type = "Type 2", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) }
            };
            _permissionRepositoryMock.Setup(repo => repo.GetPermissionsByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(permissions);

            var query = new GetPermissionsQuery { EmployeeId = employeeId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(permissions, result);
        }

        [Fact]
        public async Task Handle_ReturnsPermissions()
        {
            // Arrange
            var employeeId = 1;
            var permissions = new List<PermissionDto>
            {
                new PermissionDto { Id = 1, Description = "Test Permission", Type = "Test Type", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) }
            };

            _permissionRepositoryMock.Setup(repo => repo.GetPermissionsByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissions);

            var query = new GetPermissionsQuery { EmployeeId = employeeId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _messageServiceMock.Verify(ms => ms.PublishAsync("Get", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
