using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data;

/// <summary>
/// Adds repeatable sample data for local development. The seeded user passwords
/// are random, so these sample accounts cannot be used to sign in.
/// </summary>
public static class DatabaseSeeder
{
    private static readonly string[] MovieTitles =
    [
        "The Shawshank Redemption", "The Godfather", "The Dark Knight", "The Godfather Part II",
        "12 Angry Men", "Schindler's List", "The Lord of the Rings: The Return of the King",
        "Pulp Fiction", "The Lord of the Rings: The Fellowship of the Ring", "The Good, the Bad and the Ugly",
        "Forrest Gump", "Fight Club", "Inception", "The Lord of the Rings: The Two Towers",
        "Star Wars: The Empire Strikes Back", "The Matrix", "Goodfellas", "One Flew Over the Cuckoo's Nest",
        "Interstellar", "Seven Samurai", "Saving Private Ryan", "City of God", "The Green Mile",
        "Life Is Beautiful", "Terminator 2: Judgment Day", "Spirited Away", "Psycho", "The Pianist",
        "Parasite", "Gladiator", "The Lion King"
    ];

    // TMDb poster paths matched to the sample movie titles above.
    private static readonly IReadOnlyDictionary<string, string> MoviePosterPaths =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["The Shawshank Redemption"] = "/lyQBXzOQSuE59IsHyhrp0qIiPAz.jpg",
            ["The Godfather"] = "/3bhkrj58Vtu7enYsRolD1fZdja1.jpg",
            ["The Dark Knight"] = "/qJ2tW6WMUDux911r6m7haRef0WH.jpg",
            ["The Godfather Part II"] = "/hek3koDUyRQk7FIhPXsa6mT2Zc3.jpg",
            ["12 Angry Men"] = "/ow3wq89wM8qd5X7hWKxiRfsFf9C.jpg",
            ["Schindler's List"] = "/sF1U4EUQS8YHUYjNl3pMGNIQyr0.jpg",
            ["The Lord of the Rings: The Return of the King"] = "/rCzpDGLbOoPwLjy3OAm5NUPOTrC.jpg",
            ["Pulp Fiction"] = "/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg",
            ["The Lord of the Rings: The Fellowship of the Ring"] = "/6oom5QYQ2yQTMJIbnvbkBL9cHo6.jpg",
            ["The Good, the Bad and the Ugly"] = "/bX2xnavhMYjWDoZp1VM6VnU1xwe.jpg",
            ["Forrest Gump"] = "/arw2vcBveWOVZr6pxd9XTd1TdQa.jpg",
            ["Fight Club"] = "/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg",
            ["Inception"] = "/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg",
            ["The Lord of the Rings: The Two Towers"] = "/cMa7haLxqVe4fWNORPIq6fGdjys.jpg",
            ["Star Wars: The Empire Strikes Back"] = "/nNAeTmF4CtdSgMDplXTDPOpYzsX.jpg",
            ["The Matrix"] = "/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
            ["Goodfellas"] = "/aKuFiU82s5ISJpGZp7YkIr3kCUd.jpg",
            ["One Flew Over the Cuckoo's Nest"] = "/3jcbDmRFiQ83drXNOvRDeKHxS0C.jpg",
            ["Interstellar"] = "/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
            ["Seven Samurai"] = "/8OKmBV5BUFzmozIC3pPWKHy17kx.jpg",
            ["Saving Private Ryan"] = "/uqx37cS8cpHg8U35f9U5IBlrCV3.jpg",
            ["City of God"] = "/k7eYdWvhYQyRQoU2TB2A2Xu2TfD.jpg",
            ["The Green Mile"] = "/o0lO84GI7qrG6XFvtsPOSV7CTNa.jpg",
            ["Life Is Beautiful"] = "/74hLDKjD5aGYOotO6esUVaeISa2.jpg",
            ["Terminator 2: Judgment Day"] = "/5M0j0B18abtBI5gi2RhfjjurTqb.jpg",
            ["Spirited Away"] = "/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg",
            ["Psycho"] = "/yz4QVqPx3h1hD1DfqqQkCq3rmxW.jpg",
            ["The Pianist"] = "/2hFvxCCWrTmCYwfy7yum0GKRi3Y.jpg",
            ["Parasite"] = "/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg",
            ["Gladiator"] = "/ty8TGRuvJLPUmAR1H1nRIsgwvim.jpg",
            ["The Lion King"] = "/sKCr78MXSLixwmZ8DyJLrpMsd15.jpg"
        };

    private static readonly (string Name, string Description)[] GenreRows =
    [
        ("Action", "Fast-paced stories with physical conflict."),
        ("Adventure", "Journeys, discovery, and exploration."),
        ("Animation", "Stories created with animated imagery."),
        ("Biography", "Stories based on the lives of real people."),
        ("Comedy", "Stories built around humor."),
        ("Crime", "Stories about crime and its consequences."),
        ("Documentary", "Non-fiction films about real subjects."),
        ("Drama", "Character-focused stories and conflict."),
        ("Family", "Films made for a broad family audience."),
        ("Fantasy", "Stories featuring magical or imagined worlds."),
        ("Film Noir", "Stylized crime stories with moral ambiguity."),
        ("History", "Stories set around historical events."),
        ("Horror", "Stories designed to create fear or suspense."),
        ("Music", "Films centered on music and musicians."),
        ("Musical", "Stories that use songs and dance."),
        ("Mystery", "Stories driven by an unanswered question."),
        ("Romance", "Stories focused on romantic relationships."),
        ("Science Fiction", "Stories exploring science and imagined futures."),
        ("Sport", "Stories centered on sport and competition."),
        ("Thriller", "Tense stories with escalating danger."),
        ("War", "Stories set around armed conflict."),
        ("Western", "Stories set in the American Old West."),
        ("Psychological Thriller", "Suspense driven by characters' minds and motives."),
        ("Dark Comedy", "Comedy that treats serious or uncomfortable subjects."),
        ("Historical Drama", "Character drama in a historical setting."),
        ("Coming of Age", "Stories about growing up and self-discovery."),
        ("Superhero", "Stories about heroic characters with extraordinary abilities."),
        ("Disaster", "Stories about large-scale emergencies and survival."),
        ("Martial Arts", "Action stories centered on martial arts."),
        ("Spy", "Stories about espionage and intelligence work."),
        ("Political Drama", "Stories about political power and public life.")
    ];

    public static async Task SeedDevelopmentDataAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var seedUsers = Enumerable.Range(1, 31)
            .Select(index => new User
            {
                FirstName = $"Sample{index:00}",
                LastName = "Viewer",
                Email = $"seed.user{index:00}@movieplatform.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                PhoneNumber = $"+1000000{index:0000}",
                DateOfBirth = new DateTime(1980 + index % 25, 1 + index % 12, 1 + index % 27),
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = now
            })
            .ToArray();

        var existingEmails = await db.Users
            .Where(user => user.Email.StartsWith("seed.user"))
            .Select(user => user.Email)
            .ToListAsync(cancellationToken);
        var emailSet = existingEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);
        db.Users.AddRange(seedUsers.Where(user => !emailSet.Contains(user.Email)));

        var existingTitles = await db.Movies
            .Where(movie => MovieTitles.Contains(movie.Title))
            .Select(movie => movie.Title)
            .ToListAsync(cancellationToken);
        var titleSet = existingTitles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seedMovies = MovieTitles
            .Where(title => !titleSet.Contains(title))
            .Select((title, index) => new Movie
            {
                Title = title,
                Description = $"Sample catalog entry for {title}.",
                Genre = GenreRows[index % GenreRows.Length].Name,
                PosterUrl = GetPosterUrl(title),
                DurationMinutes = 90 + index * 3,
                ReleaseDate = new DateTime(1990 + index % 35, 1 + index % 12, 1 + index % 27),
                Director = $"Sample Director {index + 1:00}",
                Rating = 6.5m + index % 30 / 10m,
                Language = "English",
                Country = "United States",
                IsActive = true,
                CreatedAt = now
            })
            .ToArray();
        db.Movies.AddRange(seedMovies);

        // Refresh our previous placeholder URLs and fill empty sample posters,
        // but preserve any other URL or uploaded poster already assigned.
        var sampleMovies = await db.Movies
            .Where(movie => MovieTitles.Contains(movie.Title))
            .ToListAsync(cancellationToken);
        foreach (var movie in sampleMovies.Where(movie =>
                     string.IsNullOrWhiteSpace(movie.PosterUrl) ||
                     movie.PosterUrl.StartsWith("https://placehold.co/", StringComparison.OrdinalIgnoreCase)))
        {
            movie.PosterUrl = GetPosterUrl(movie.Title);
            movie.UpdatedAt = now;
        }

        var genreNames = GenreRows.Select(row => row.Name).ToArray();
        var existingGenreNames = await db.Genres
            .Where(genre => genreNames.Contains(genre.Name))
            .Select(genre => genre.Name)
            .ToListAsync(cancellationToken);
        var genreSet = existingGenreNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        db.Genres.AddRange(GenreRows
            .Where(row => !genreSet.Contains(row.Name))
            .Select(row => new Genre { Name = row.Name, Description = row.Description, IsActive = true, CreatedAt = now }));

        await db.SaveChangesAsync(cancellationToken);

        var seedEmails = Enumerable.Range(1, 31).Select(index => $"seed.user{index:00}@movieplatform.local").ToArray();
        var usersByEmail = await db.Users
            .Where(user => seedEmails.Contains(user.Email))
            .ToDictionaryAsync(user => user.Email, cancellationToken);
        var moviesByTitle = await db.Movies
            .Where(movie => MovieTitles.Contains(movie.Title))
            .ToDictionaryAsync(movie => movie.Title, cancellationToken);

        var samplePairs = Enumerable.Range(1, 31)
            .Select(index => (
                UserId: usersByEmail[$"seed.user{index:00}@movieplatform.local"].Id,
                MovieId: moviesByTitle[MovieTitles[index - 1]].Id,
                Index: index))
            .ToArray();
        var userIds = samplePairs.Select(pair => pair.UserId).ToArray();
        var movieIds = samplePairs.Select(pair => pair.MovieId).ToArray();

        var reviewPairs = (await db.Reviews
                .Where(review => userIds.Contains(review.UserId) && movieIds.Contains(review.MovieId))
                .Select(review => new { review.UserId, review.MovieId })
                .ToListAsync(cancellationToken))
            .Select(pair => (pair.UserId, pair.MovieId))
            .ToHashSet();
        db.Reviews.AddRange(samplePairs
            .Where(pair => !reviewPairs.Contains((pair.UserId, pair.MovieId)))
            .Select(pair => new Review
            {
                UserId = pair.UserId,
                MovieId = pair.MovieId,
                Rating = 1 + pair.Index % 10,
                Comment = $"Sample review {pair.Index:00}.",
                IsActive = true,
                CreatedAt = now
            }));

        var watchlistPairs = (await db.Watchlists
                .Where(item => userIds.Contains(item.UserId) && movieIds.Contains(item.MovieId))
                .Select(item => new { item.UserId, item.MovieId })
                .ToListAsync(cancellationToken))
            .Select(item => (item.UserId, item.MovieId))
            .ToHashSet();
        db.Watchlists.AddRange(samplePairs
            .Where(pair => !watchlistPairs.Contains((pair.UserId, pair.MovieId)))
            .Select(pair => new Watchlist
            {
                UserId = pair.UserId,
                MovieId = pair.MovieId,
                IsWatched = pair.Index % 2 == 0,
                WatchedAt = pair.Index % 2 == 0 ? now.AddDays(-pair.Index) : null,
                IsActive = true,
                CreatedAt = now
            }));

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string GetPosterUrl(string title) =>
        $"https://image.tmdb.org/t/p/w500{MoviePosterPaths[title]}";
}
