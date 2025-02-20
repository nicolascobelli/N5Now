using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Repositories;
using UserPermissions.Infrastructure.Data;

namespace UserPermissions.Infrastructure.Repositories
{
    public class PermissionsReadRepository : IPermissionsReadRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionsReadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PermissionDto>> GetPermissionsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken)
        {
            var permissions = await _context.Permissions
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync(cancellationToken);

            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Description = p.Description,
                Type = p.PermissionType.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList();
        }
    }
}
