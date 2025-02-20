using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application.Repositories
{
    public interface IPermissionTypeReadRepository
    {
        Task<PermissionType> GetPermissionTypeByIdAsync(int permissionTypeId, CancellationToken cancellationToken);
    }
}
