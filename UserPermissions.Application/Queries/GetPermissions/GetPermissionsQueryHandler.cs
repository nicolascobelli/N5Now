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
        private readonly IPermissionReadRepository _permissionRepository;
        private readonly IMessageService _messageService;

        public GetPermissionsQueryHandler(IPermissionReadRepository permissionRepository, IMessageService messageService)
        {
            _permissionRepository = permissionRepository;
            _messageService = messageService;
        }

        public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _permissionRepository.GetAllPermissionsAsync(cancellationToken);
            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
            }).ToList();

            await _messageService.PublishAsync("GetPermissions", cancellationToken);

            //i dont know what to send to elastic here, all of them?

            return permissionDtos;
        }
    }
}