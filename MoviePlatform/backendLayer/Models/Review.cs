using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementApi.Models;

public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    public int MovieId { get; set; }

    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;

    [Required]
    [Range(1, 10)]
    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}