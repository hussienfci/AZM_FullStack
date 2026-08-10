using AutoMapper;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Domain.Entities;

namespace MoviePlatform.Modules.Catalog.Application.Mappings;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<Movie, MovieSummaryDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => 
                src.MovieGenres.Select(mg => mg.Genre.Name).ToList()));

        CreateMap<Movie, MovieDetailsDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => 
                src.MovieGenres.Select(mg => new GenreDto 
                { 
                    Id = mg.Genre.Id, 
                    Name = mg.Genre.Name,
                    Description = mg.Genre.Description
                }).ToList()));

        CreateMap<Genre, GenreDto>();
    }
}
