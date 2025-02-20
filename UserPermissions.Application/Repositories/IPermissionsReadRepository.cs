using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.DTOs;

namespace UserPermissions.Application.Repositories
{
    public interface IPermissionsReadRepository
    {
        Task<List<PermissionDto>> GetPermissionsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken);
    }
}
