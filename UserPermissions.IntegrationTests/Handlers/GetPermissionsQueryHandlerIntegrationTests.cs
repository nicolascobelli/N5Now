using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserPermissions.Application.Queries.GetPermissions;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.DTOs;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using UserPermissions.Infrastructure.Repositories;
using Xunit;
using UserPermissions.Application.Services;
using UserPermissions.IntegrationTests;


namespace UserPermissions.IntegrationTests.Handlers
{
    public class GetPermissionsQueryHandlerIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionsReadRepository _repository;
        private readonly GetPermissionsQueryHandler _handler;
        private readonly Mock<IMessageService> _messageServiceMock;

        public GetPermissionsQueryHandlerIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _repository = new PermissionsReadRepository(_context);
            _messageServiceMock = fixture.MessageServiceMock;
            _handler = new GetPermissionsQueryHandler(_repository, _messageServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsPermissionsByEmployee()
        {
            // Arrange
            var employee = new Employee { Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Name = "Test Permission Type" };
            var employee2 = new Employee { Name = "Test Employee2", Email = "test@example.com" };

            await _context.Employees.AddAsync(employee);
            await _context.Employees.AddAsync(employee2);
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var p1 = new Permission("Permission 1", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));
            var p2 = new Permission("Permission 2", employee2.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));

            await _context.Permissions.AddAsync(p1);
            await _context.Permissions.AddAsync(p2);
            await _context.SaveChangesAsync();

            var query = new GetPermissionsQuery { EmployeeId = employee.Id };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var expected = new PermissionDto()
            {
                Id = p1.Id,
                Description = p1.Description,
                Type = p1.PermissionType.Name,
                StartDate = p1.StartDate,
                EndDate = p1.EndDate
            };

            Assert.Single(result);
            Assert.Equal(expected.Id, result.First().Id);

            // Verify message service
            _messageServiceMock.Verify(ms => ms.PublishAsync("Get", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
