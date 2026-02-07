using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.API.Models.Entities;

public class Notification
{
    [Key]
    public long Id { get; set; }

    public long UserId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ReadAt { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
