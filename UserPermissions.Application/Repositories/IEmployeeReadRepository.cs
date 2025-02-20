using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application.Repositories
{
    public interface IEmployeeReadRepository
    {
        Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken);
    }
}
