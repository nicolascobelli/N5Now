using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Serilog;
using UserPermissions.Application.Commands.RequestPermission;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserPermissions.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Nest;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserPermissions.API", Version = "v1" });
});

// Add MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// Configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Elasticsearch
var elasticsearchUrl = builder.Configuration["ElasticSearch:Url"];
if (string.IsNullOrEmpty(elasticsearchUrl))
{
    throw new InvalidOperationException("Elasticsearch URL is not configured.");
}
var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
    .DefaultIndex("userpermissions");
var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);

// Configure Kafka
var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
};
builder.Services.AddSingleton(producerConfig);
builder.Services.AddSingleton<IProducer<string, string>>(sp => new ProducerBuilder<string, string>(producerConfig).Build());

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserPermissions.API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
