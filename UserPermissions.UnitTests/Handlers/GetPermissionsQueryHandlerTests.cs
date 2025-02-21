using System;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Queries.GetPermissions;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.DTOs;
using UserPermissions.Domain.Entities;
using Xunit;
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
        public async Task Handle_ReturnsAllPermissions()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                new Permission("Permission 1", 1, 1, DateTime.Now, DateTime.Now.AddDays(1)),
                new Permission("Permission 2", 1, 1, DateTime.Now, DateTime.Now.AddDays(1))
            };
            _permissionRepositoryMock.Setup(repo => repo.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissions);

            var query = new GetPermissionsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(permissions.Select(p =>p.Id).ToList(), result.Select(r => r.Id).ToList());
            _messageServiceMock.Verify(ms => ms.PublishAsync("GetPermissions", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
