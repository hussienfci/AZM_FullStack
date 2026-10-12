using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Models;

public class Movie
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Genre { get; set; } = string.Empty;

    public int? DurationMinutes { get; set; }

    public DateTime? ReleaseDate { get; set; }

    [StringLength(150)]
    public string? Director { get; set; }

    [StringLength(500)]
    public string? PosterUrl { get; set; }

    [StringLength(500)]
    public string? TrailerUrl { get; set; }

    public decimal? Rating { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}