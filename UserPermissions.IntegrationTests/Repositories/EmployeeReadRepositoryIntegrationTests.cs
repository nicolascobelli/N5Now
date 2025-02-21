using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using UserPermissions.Infrastructure.Repositories;
using Xunit;

namespace UserPermissions.IntegrationTests.Repositories
{
    public class EmployeeReadRepositoryIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeeReadRepository _repository;

        public EmployeeReadRepositoryIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _repository = new EmployeeReadRepository(_context);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Test Employee", Email = "test@example.com" };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeeByIdAsync(1, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Employee", result.Name);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ShouldReturnNull_WhenEmployeeDoesNotExist()
        {
            // Act
            var result = await _repository.GetEmployeeByIdAsync(999, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
