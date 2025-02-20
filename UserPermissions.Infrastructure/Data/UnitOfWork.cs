using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.Repositories;
using UserPermissions.Infrastructure.Repositories;

namespace UserPermissions.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IEmployeeReadRepository _employeeReadRepository = null!;
        private IPermissionRepository _permissionRepository = null!;
        private IPermissionTypeReadRepository _permissionTypeReadRepository = null!;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEmployeeReadRepository EmployeeReadRepository => _employeeReadRepository ??= new EmployeeReadRepository(_context);
        public IPermissionRepository PermissionRepository => _permissionRepository ??= new PermissionRepository(_context);
        public IPermissionTypeReadRepository PermissionTypeReadRepository => _permissionTypeReadRepository ??= new PermissionTypeReadRepository(_context);

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
