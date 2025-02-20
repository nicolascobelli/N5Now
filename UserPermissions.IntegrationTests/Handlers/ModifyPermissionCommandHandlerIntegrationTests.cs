using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserPermissions.Application.Commands.ModifyPermission;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using UserPermissions.Infrastructure.Repositories;
using Xunit;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Nest;

namespace UserPermissions.IntegrationTests.Handlers
{
    public class ModifyPermissionCommandHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ModifyPermissionCommandHandler _handler;
        private readonly Mock<IProducer<string, string>> _producerMock;
        private readonly Mock<IElasticClient> _elasticClientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly UnitOfWork _unitOfWork;

        public ModifyPermissionCommandHandlerIntegrationTests()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _unitOfWork = new UnitOfWork(_context);
            _producerMock = new Mock<IProducer<string, string>>();
            _elasticClientMock = new Mock<IElasticClient>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Kafka:TopicName"]).Returns("test-topic");

            _handler = new ModifyPermissionCommandHandler(
                _unitOfWork,
                _producerMock.Object,
                _elasticClientMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_UpdatesPermission()
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

            var command = new ModifyPermissionCommand
            {
                EmployeeId = employee.Id,
                PermissionId = permission.Id,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(3)
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            var updatedPermission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permission.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPermission);
            Assert.Equal(command.StartDate, updatedPermission.StartDate);
            Assert.Equal(command.EndDate, updatedPermission.EndDate);

            // Verify Kafka message
            _producerMock.Verify(producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);

            // Verify Elasticsearch indexing
            _elasticClientMock.Verify(ec => ec.IndexDocumentAsync(It.Is<Permission>(p => p.Id == permission.Id), It.IsAny<CancellationToken>()), Times.Once);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
