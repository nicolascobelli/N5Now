using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserPermissions.Application.Queries.GetPermissions;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.DTOs;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using UserPermissions.Infrastructure.Repositories;
using UserPermissions.Application.Services;
using Xunit;

namespace UserPermissions.IntegrationTests.Handlers
{
    public class GetPermissionsQueryHandlerIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionReadRepository _repository;
        private readonly GetPermissionsQueryHandler _handler;
        private readonly Mock<IMessageService> _messageServiceMock;

        public GetPermissionsQueryHandlerIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _repository = new PermissionReadRepository(_context);
            _messageServiceMock = fixture.MessageServiceMock;
            _handler = new GetPermissionsQueryHandler(_repository, _messageServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAllPermissions()
        {
            // Arrange
            var employee = new Employee { Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Name = "Test Permission Type" };

            await _context.Employees.AddAsync(employee);
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var p1 = new Permission("Permission 1", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));
            var p2 = new Permission("Permission 2", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));

            await _context.Permissions.AddAsync(p1);
            await _context.Permissions.AddAsync(p2);
            await _context.SaveChangesAsync();

            var query = new GetPermissionsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Description == "Permission 1");
            Assert.Contains(result, r => r.Description == "Permission 2");

            // Verify message service
            _messageServiceMock.Verify(ms => ms.PublishAsync("GetPermissions", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
