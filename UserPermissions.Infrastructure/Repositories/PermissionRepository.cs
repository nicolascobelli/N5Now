using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;
using System.Collections.Generic;

namespace UserPermissions.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken)
        {
            await _context.Permissions.AddAsync(permission, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken)
        {
            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Permission?> GetPermissionByIdAndEmployeeIdAsync(int permissionId, int employeeId, CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == permissionId && p.EmployeeId == employeeId, cancellationToken)!;
        }
    }
}
