using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application.Repositories
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken);
        Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken);
        Task UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken);
    }
}
