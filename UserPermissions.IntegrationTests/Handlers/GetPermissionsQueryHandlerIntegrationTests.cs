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
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Nest;

namespace UserPermissions.IntegrationTests.Handlers
{
    public class GetPermissionsQueryHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GetPermissionsQueryHandler _handler;
        private readonly Mock<IProducer<string, string>> _producerMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public GetPermissionsQueryHandlerIntegrationTests()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            var permissionRepository = new PermissionsReadRepository(_context);
            _producerMock = new Mock<IProducer<string, string>>();
            _configurationMock = new Mock<IConfiguration>();

            _handler = new GetPermissionsQueryHandler(permissionRepository, _producerMock.Object, _configurationMock.Object);
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

            Assert.Equal(1, result.Count());
            Assert.Equal(expected.Id, result.First().Id);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
