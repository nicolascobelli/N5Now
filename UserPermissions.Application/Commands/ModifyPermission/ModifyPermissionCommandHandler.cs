using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Nest;

namespace UserPermissions.Application.Commands.ModifyPermission
{
    public class ModifyPermissionCommandHandler : IRequestHandler<ModifyPermissionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProducer<string, string> _producer;
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _configuration;

        public ModifyPermissionCommandHandler(
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

        public async Task<bool> Handle(ModifyPermissionCommand request, CancellationToken cancellationToken)
        {
            var employee = await _unitOfWork.EmployeeReadRepository.GetEmployeeByIdAsync(request.EmployeeId, cancellationToken);
            if (employee == null)
            {
                return false; // Employee not found
            }

            var permission = await _unitOfWork.PermissionRepository.GetPermissionByIdAsync(request.PermissionId, cancellationToken);
            if (permission == null)
            {
                return false; // Permission not found
            }

            permission.Update(request.StartDate, request.EndDate);
            await _unitOfWork.PermissionRepository.UpdatePermissionAsync(permission, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            // Produce Kafka message
            var topic = _configuration["Kafka:Topic"];
            var message = new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = "Modify Permission" };
            await _producer.ProduceAsync(topic, message, cancellationToken);

            // Index document in Elasticsearch
            await _elasticClient.IndexDocumentAsync(permission, cancellationToken);

            return true;
        }
    }
}
