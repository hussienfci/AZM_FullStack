using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Shared.Kernel.Interfaces;

namespace MoviePlatform.Shared.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
