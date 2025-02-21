using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using UserPermissions.Infrastructure.Repositories;
using Xunit;

namespace UserPermissions.IntegrationTests.Repositories
{
    public class PermissionTypeReadRepositoryIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private readonly PermissionTypeReadRepository _repository;

        public PermissionTypeReadRepositoryIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _repository = new PermissionTypeReadRepository(_context);
        }

        [Fact]
        public async Task GetPermissionTypeByIdAsync_ShouldReturnPermissionType_WhenPermissionTypeExists()
        {
            // Arrange
            var permissionType = new PermissionType { Id = 1, Name = "Test Permission Type" };
            await _context.PermissionTypes.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPermissionTypeByIdAsync(1, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Permission Type", result.Name);
        }

        [Fact]
        public async Task GetPermissionTypeByIdAsync_ShouldReturnNull_WhenPermissionTypeDoesNotExist()
        {
            // Act
            var result = await _repository.GetPermissionTypeByIdAsync(999, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
