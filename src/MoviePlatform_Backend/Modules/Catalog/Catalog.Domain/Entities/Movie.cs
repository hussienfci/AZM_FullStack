using System;
using System.Collections.Generic;
using MoviePlatform.Shared.Kernel.Entities;

namespace MoviePlatform.Modules.Catalog.Domain.Entities;

public class Movie : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? PosterUrl { get; private set; }
    public string? BackdropUrl { get; private set; }
    public string? TrailerUrl { get; private set; }
    public int ReleaseYear { get; private set; }
    public int? DurationMinutes { get; private set; }
    public string? Director { get; private set; }
    public string? Language { get; private set; }
    public decimal? AverageRating { get; private set; }
    public int ReviewCount { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTime? FeaturedUntil { get; private set; }

    private readonly List<MovieGenre> _movieGenres = new();
    public IReadOnlyCollection<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();

    private Movie() { } // EF Core protected/private constructor

    public static Movie Create(
        string title,
        string description,
        int releaseYear,
        string? posterUrl = null,
        string? backdropUrl = null,
        string? trailerUrl = null,
        int? durationMinutes = null,
        string? director = null,
        string? language = null)
    {
        var movie = new Movie
        {
            Title = title,
            Description = description,
            ReleaseYear = releaseYear,
            PosterUrl = posterUrl,
            BackdropUrl = backdropUrl,
            TrailerUrl = trailerUrl,
            DurationMinutes = durationMinutes,
            Director = director,
            Language = language,
            AverageRating = 0,
            ReviewCount = 0,
            IsFeatured = false
        };

        movie.AddDomainEvent(new MovieCreatedEvent(movie.Id, movie.Title));
        return movie;
    }

    public void UpdateDetails(
        string title,
        string description,
        int releaseYear,
        string? posterUrl = null,
        string? backdropUrl = null,
        string? trailerUrl = null,
        int? durationMinutes = null,
        string? director = null,
        string? language = null)
    {
        Title = title;
        Description = description;
        ReleaseYear = releaseYear;
        PosterUrl = posterUrl;
        BackdropUrl = backdropUrl;
        TrailerUrl = trailerUrl;
        DurationMinutes = durationMinutes;
        Director = director;
        Language = language;
        UpdateTimestamp();
    }

    public void AddGenre(Genre genre)
    {
        if (!_movieGenres.Any(mg => mg.GenreId == genre.Id))
        {
            _movieGenres.Add(new MovieGenre { MovieId = Id, GenreId = genre.Id });
        }
    }

    public void RemoveGenre(Guid genreId)
    {
        var movieGenre = _movieGenres.FirstOrDefault(mg => mg.GenreId == genreId);
        if (movieGenre != null)
        {
            _movieGenres.Remove(movieGenre);
        }
    }

    public void SetFeatured(DateTime? featuredUntil = null)
    {
        IsFeatured = true;
        FeaturedUntil = featuredUntil ?? DateTime.UtcNow.AddMonths(1);
        UpdateTimestamp();
    }

    public void RemoveFeatured()
    {
        IsFeatured = false;
        FeaturedUntil = null;
        UpdateTimestamp();
    }

    public void UpdateRating(decimal averageRating, int reviewCount)
    {
        AverageRating = averageRating;
        ReviewCount = reviewCount;
        UpdateTimestamp();
    }
}
