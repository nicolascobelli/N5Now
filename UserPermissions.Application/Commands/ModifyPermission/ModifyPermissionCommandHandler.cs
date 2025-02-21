using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.Services;
using UserPermissions.Domain.Entities;
using Nest;

namespace UserPermissions.Application.Commands.ModifyPermission
{
    public class ModifyPermissionCommandHandler : IRequestHandler<ModifyPermissionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private readonly IElasticClient _elasticClient;

        public ModifyPermissionCommandHandler(
            IUnitOfWork unitOfWork,
            IMessageService messageService,
            IElasticClient elasticClient)
        {
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _elasticClient = elasticClient;
        }

        public async Task<bool> Handle(ModifyPermissionCommand request, CancellationToken cancellationToken)
        {
            var employee = await _unitOfWork.EmployeeReadRepository.GetEmployeeByIdAsync(request.EmployeeId, cancellationToken);
            if (employee == null)
            {
                return false; // Employee not found
            }

            var permission = await _unitOfWork.PermissionRepository.GetPermissionByIdAndEmployeeIdAsync(request.PermissionId, request.EmployeeId, cancellationToken);
            if (permission == null)
            {
                return false; // Permission not found
            }

            permission.Update(request.StartDate, request.EndDate);
            await _unitOfWork.PermissionRepository.UpdatePermissionAsync(permission, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            // Publish Kafka message
            await _messageService.PublishAsync("Modify", cancellationToken);

            // Index document in Elasticsearch
            await _elasticClient.IndexDocumentAsync(permission);

            return true;
        }
    }
}