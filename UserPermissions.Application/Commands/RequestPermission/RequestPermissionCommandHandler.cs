using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Nest;

namespace UserPermissions.Application.Commands.RequestPermission
{
    public class RequestPermissionCommandHandler : IRequestHandler<RequestPermissionCommand, int?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProducer<string, string> _producer;
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _configuration;

        public RequestPermissionCommandHandler(
            IUnitOfWork unitOfWork,
            IProducer<string, string> producer,
            IElasticClient elasticClient,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _producer = producer;
            _elasticClient = elasticClient;
            _configuration = configuration;
        }

        public async Task<int?> Handle(RequestPermissionCommand request, CancellationToken cancellationToken)
        {
            var employee = await _unitOfWork.EmployeeReadRepository.GetEmployeeByIdAsync(request.EmployeeId, cancellationToken);
            if (employee == null)
            {
                return null; // Employee not found
            }

            var permissionType = await _unitOfWork.PermissionTypeReadRepository.GetPermissionTypeByIdAsync(request.PermissionTypeId, cancellationToken);
            if (permissionType == null)
            {
                return null; // Permission type not found
            }

            var permission = new Permission(request.Description, request.EmployeeId, request.PermissionTypeId, request.StartDate, request.EndDate);
            await _unitOfWork.PermissionRepository.AddPermissionAsync(permission, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            // Produce Kafka message
            var topic = _configuration["Kafka:TopicName"];
            var message = new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = "Permission requested" };
            await _producer.ProduceAsync(topic, message, cancellationToken);

            // Index document in Elasticsearch
            await _elasticClient.IndexDocumentAsync(permission, cancellationToken);

            return permission.Id;
        }
    }
}