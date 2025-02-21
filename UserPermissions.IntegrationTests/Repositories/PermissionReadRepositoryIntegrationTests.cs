using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using UserPermissions.Infrastructure.Repositories;
using Xunit;

namespace UserPermissions.IntegrationTests.Repositories
{
    public class PermissionReadRepositoryIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private readonly PermissionReadRepository _repository;

        public PermissionReadRepositoryIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _repository = new PermissionReadRepository(_context);
        }

        [Fact]
        public async Task GetAllPermissionsAsync_ShouldReturnAllPermissions()
        {
            // Arrange
            var employee = new Employee { Name = "John Doe" };
            var permissionType = new PermissionType { Name = "Type 1" };
            await _context.Employees.AddAsync(employee);
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permissions = new List<Permission>
            {
                new Permission("Permission 1", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1)),
                new Permission("Permission 2", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1))
            };
            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllPermissionsAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
