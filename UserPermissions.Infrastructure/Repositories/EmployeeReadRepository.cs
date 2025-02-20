using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Repositories;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Data;

namespace UserPermissions.Infrastructure.Repositories
{
    public class EmployeeReadRepository : IEmployeeReadRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeReadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        }
    }
}
