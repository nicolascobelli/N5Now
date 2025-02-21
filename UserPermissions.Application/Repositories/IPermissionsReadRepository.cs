using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application.Repositories
{
    public interface IPermissionsReadRepository
    {
        Task<List<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken);
    }
}
