using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Modules.Catalog.Domain.Entities;

namespace MoviePlatform.Modules.Catalog.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(CatalogDbContext context)
    {
        if (await context.Genres.AnyAsync())
            return; // Already seeded

        var genres = new List<Genre>
        {
            Genre.Create("Action", "Fast-paced movies with exciting sequences"),
            Genre.Create("Adventure", "Journey and exploration stories"),
            Genre.Create("Animation", "Animated films for all ages"),
            Genre.Create("Comedy", "Humorous and entertaining movies"),
            Genre.Create("Crime", "Stories about criminals and law enforcement"),
            Genre.Create("Drama", "Serious and emotional narratives"),
            Genre.Create("Fantasy", "Magical and supernatural worlds"),
            Genre.Create("Horror", "Scary and suspenseful films"),
            Genre.Create("Mystery", "Puzzles and unexplained events"),
            Genre.Create("Romance", "Love stories and relationships"),
            Genre.Create("Sci-Fi", "Science fiction and futuristic tales"),
            Genre.Create("Thriller", "Tense and exciting narratives")
        };

        await context.Genres.AddRangeAsync(genres);
        await context.SaveChangesAsync();

        var action = genres.First(g => g.Name == "Action");
        var adventure = genres.First(g => g.Name == "Adventure");
        var sciFi = genres.First(g => g.Name == "Sci-Fi");
        var drama = genres.First(g => g.Name == "Drama");
        var crime = genres.First(g => g.Name == "Crime");
        var comedy = genres.First(g => g.Name == "Comedy");
        var thriller = genres.First(g => g.Name == "Thriller");
        var fantasy = genres.First(g => g.Name == "Fantasy");

        var movies = new List<Movie>
        {
            CreateMovie("Inception", "A thief who steals corporate secrets through dream-sharing technology", 2010, 
                "Christopher Nolan", 148, new[] { action, sciFi, thriller }),
            CreateMovie("The Dark Knight", "Batman faces the Joker, a criminal mastermind", 2008,
                "Christopher Nolan", 152, new[] { action, crime, drama }),
            CreateMovie("Interstellar", "A team travels through a wormhole in space", 2014,
                "Christopher Nolan", 169, new[] { sciFi, adventure, drama }),
            CreateMovie("The Matrix", "A computer hacker learns about the true nature of reality", 1999,
                "The Wachowskis", 136, new[] { action, sciFi }),
            CreateMovie("Pulp Fiction", "The lives of two mob hitmen, a boxer, and others intertwine", 1994,
                "Quentin Tarantino", 154, new[] { crime, drama }),
            CreateMovie("The Shawshank Redemption", "Two imprisoned men bond over years", 1994,
                "Frank Darabont", 142, new[] { drama }),
            CreateMovie("Forrest Gump", "The story of a man with a low IQ who achieves great things", 1994,
                "Robert Zemeckis", 142, new[] { drama, comedy }),
            CreateMovie("The Lord of the Rings: The Fellowship of the Ring", "A hobbit and companions set out to destroy a powerful ring", 2001,
                "Peter Jackson", 178, new[] { adventure, fantasy, action }),
        };

        // Set some as featured
        movies[0].SetFeatured();
        movies[1].SetFeatured();
        movies[2].SetFeatured();

        // Set ratings
        movies[0].UpdateRating(8.8m, 2500);
        movies[1].UpdateRating(9.0m, 3200);
        movies[2].UpdateRating(8.6m, 2100);
        movies[3].UpdateRating(8.7m, 2800);
        movies[4].UpdateRating(8.9m, 2400);
        movies[5].UpdateRating(9.3m, 4100);
        movies[6].UpdateRating(8.8m, 2300);
        movies[7].UpdateRating(8.8m, 1900);

        await context.Movies.AddRangeAsync(movies);
        await context.SaveChangesAsync();
    }

    private static Movie CreateMovie(string title, string description, int year, string director, int duration, Genre[] genres)
    {
        var movie = Movie.Create(title, description, year, 
            posterUrl: $"https://image.tmdb.org/t/p/w500/sample-{title.ToLower().Replace(" ", "-")}.jpg",
            director: director,
            durationMinutes: duration);

        foreach (var genre in genres)
        {
            movie.AddGenre(genre);
        }

        return movie;
    }
}
