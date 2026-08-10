using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MoviePlatform.Modules.Catalog.Domain.Entities;
using MoviePlatform.Shared.Kernel.Interfaces;

namespace MoviePlatform.Modules.Catalog.Domain.Interfaces;

public interface IGenreRepository : IRepository<Genre>
{
    Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
