using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.Services;
using UserPermissions.Domain.Entities;
using Nest;

namespace UserPermissions.Application.Commands.RequestPermission
{
    public class RequestPermissionCommandHandler : IRequestHandler<RequestPermissionCommand, int?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private readonly IElasticClient _elasticClient;

        public RequestPermissionCommandHandler(
            IUnitOfWork unitOfWork,
            IMessageService messageService,
            IElasticClient elasticClient)
        {
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _elasticClient = elasticClient;
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

            // Publish Kafka message
            await _messageService.PublishAsync("Request", cancellationToken);

            // Index document in Elasticsearch
            await _elasticClient.IndexDocumentAsync(permission);

            return permission.Id;
        }
    }
}