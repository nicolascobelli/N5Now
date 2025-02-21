using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;

namespace UserPermissions.Infrastructure.Repositories
{
    public class PermissionTypeReadRepository : IPermissionTypeReadRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionTypeReadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PermissionType?> GetPermissionTypeByIdAsync(int permissionTypeId, CancellationToken cancellationToken)
        {
            return await _context.PermissionTypes.AsNoTracking()
                .FirstOrDefaultAsync(pt => pt.Id == permissionTypeId, cancellationToken);
        }
    }
}
