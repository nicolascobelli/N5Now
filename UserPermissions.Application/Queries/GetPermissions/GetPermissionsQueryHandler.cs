using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Repositories;
using UserPermissions.Application.Services;

namespace UserPermissions.Application.Queries.GetPermissions
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
    {
        private readonly IPermissionsReadRepository _permissionRepository;
        private readonly IMessageService _messageService;

        public GetPermissionsQueryHandler(IPermissionsReadRepository permissionRepository, IMessageService messageService)
        {
            _permissionRepository = permissionRepository;
            _messageService = messageService;
        }

        public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _permissionRepository.GetPermissionsByEmployeeIdAsync(request.EmployeeId, cancellationToken);

            // Publish Kafka message
            await _messageService.PublishAsync("Get", cancellationToken);

            //i dont understand what to send in elastic here.

            return permissions;
        }
    }
}