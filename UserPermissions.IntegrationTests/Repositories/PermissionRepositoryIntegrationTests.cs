using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data; 
using UserPermissions.Infrastructure.Repositories;
using UserPermissions.IntegrationTests;
using Xunit;

namespace UserPermissions.IntegrationTests.Repositories
{
    public class PermissionRepositoryIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private readonly PermissionRepository _repository;

        public PermissionRepositoryIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _repository = new PermissionRepository(_context);
        }

        [Fact]
        public async Task AddPermissionAsync_AddsPermission()
        {
            // Arrange
            var employee = new Employee { Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Name = "Test Permission Type" };
            await _context.Employees.AddAsync(employee);
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permission = new Permission("New Permission", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));

            // Act
            await _repository.AddPermissionAsync(permission, CancellationToken.None);
            var result = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permission.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permission.Description, result.Description);
        }

        [Fact]
        public async Task UpdatePermissionAsync_UpdatesPermission()
        {
            // Arrange
            var employee = new Employee { Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Name = "Test Permission Type" };
            await _context.Employees.AddAsync(employee);
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permission = new Permission("Existing Permission", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();

            // Act
            permission.Update(DateTime.Now.AddDays(2), DateTime.Now.AddDays(3));
            await _repository.UpdatePermissionAsync(permission, CancellationToken.None);
            var result = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permission.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permission.StartDate, result.StartDate);
            Assert.Equal(permission.EndDate, result.EndDate);
        }

        [Fact]
        public async Task GetPermissionByIdAndEmployeeIdAsync_ShouldReturnPermission_WhenPermissionExists()
        {
            // Arrange
            var employee = new Employee { Name = "Test Employee", Email = "test@example.com" };
            var permissionType = new PermissionType { Name = "Test Permission Type" };
            await _context.Employees.AddAsync(employee);
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permission = new Permission("Existing Permission", employee.Id, permissionType.Id, DateTime.Now, DateTime.Now.AddDays(1));
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPermissionByIdAndEmployeeIdAsync(permission.Id, employee.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permission.Id, result.Id);
            Assert.Equal(employee.Id, result.EmployeeId);
        }

        [Fact]
        public async Task GetPermissionByIdAndEmployeeIdAsync_ShouldReturnNull_WhenPermissionDoesNotExist()
        {
            // Act
            var result = await _repository.GetPermissionByIdAndEmployeeIdAsync(999, 999, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
