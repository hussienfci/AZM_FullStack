using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetAllGenres;

public class GetAllGenresHandler : IRequestHandler<GetAllGenresQuery, Result<IReadOnlyList<GenreDto>>>
{
    private readonly IGenreRepository _genreRepository;
    private readonly IMapper _mapper;

    public GetAllGenresHandler(IGenreRepository genreRepository, IMapper mapper)
    {
        _genreRepository = genreRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<GenreDto>>> Handle(GetAllGenresQuery request, CancellationToken cancellationToken)
    {
        var genres = await _genreRepository.GetAllAsync(cancellationToken);
        var genreDtos = _mapper.Map<IReadOnlyList<GenreDto>>(genres);
        return Result<IReadOnlyList<GenreDto>>.Success(genreDtos);
    }
}
