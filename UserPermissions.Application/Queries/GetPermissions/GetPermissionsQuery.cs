using MediatR;
using System.Collections.Generic;
using UserPermissions.Application.DTOs;

namespace UserPermissions.Application.Queries.GetPermissions
{
    public class GetPermissionsQuery : IRequest<List<PermissionDto>>
    {
    }
}
