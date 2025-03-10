using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;

namespace UserPermissions.Infrastructure.Repositories
{
    public class PermissionReadRepository : IPermissionReadRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionReadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

         public async Task<List<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
