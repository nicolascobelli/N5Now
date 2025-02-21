using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nest;
using UserPermissions.Application.Services;
using UserPermissions.Infrastructure.Data;

namespace UserPermissions.IntegrationTests
{
    public class IntegrationTestFixture : IDisposable
    {
        public ApplicationDbContext Context { get; }
        public Mock<IMessageService> MessageServiceMock { get; }
        public Mock<IElasticClient> ElasticClientMock { get; }

        public IntegrationTestFixture()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            Context = new ApplicationDbContext(options);
            Context.Database.EnsureCreated();

            MessageServiceMock = new Mock<IMessageService>();
            ElasticClientMock = new Mock<IElasticClient>();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
