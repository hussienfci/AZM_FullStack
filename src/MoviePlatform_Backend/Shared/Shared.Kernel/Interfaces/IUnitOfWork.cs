using System.Threading;
using System.Threading.Tasks;

namespace MoviePlatform.Shared.Kernel.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
