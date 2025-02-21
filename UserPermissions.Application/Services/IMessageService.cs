using System.Threading;
using System.Threading.Tasks;

namespace UserPermissions.Application.Services
{
    public interface IMessageService
    {
        Task PublishAsync(string message, CancellationToken cancellationToken);
    }
}
