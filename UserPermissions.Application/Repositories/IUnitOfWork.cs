using System;
using System.Threading;
using System.Threading.Tasks;

namespace UserPermissions.Application.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IEmployeeReadRepository EmployeeReadRepository { get; }
        IPermissionRepository PermissionRepository { get; }
        IPermissionTypeReadRepository PermissionTypeReadRepository { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
