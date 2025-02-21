using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application.Repositories
{
    public interface IPermissionRepository
    {
        Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken);
        Task UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken);
        Task<Permission?> GetPermissionByIdAndEmployeeIdAsync(int permissionId, int employeeId, CancellationToken cancellationToken);
    }
}
