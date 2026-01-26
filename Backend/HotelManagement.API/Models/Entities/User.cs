// File: Models/Entities/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.API.Models.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("username")]
        public string Username { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        // ENUM('Admin','Manager','Receptionist','Housekeeping')
        [Required]
        [Column("role")]
        public string Role { get; set; } = null!;

        [MaxLength(128)]
        [Column("email")]
        public string? Email { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation: một User có thể được assign nhiều housekeeping_tasks
        public virtual ICollection<HousekeepingTask> HousekeepingTasks { get; set; }
            = new List<HousekeepingTask>();

        // Navigation: một User có thể có nhiều RefreshTokens
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();
    }
}
