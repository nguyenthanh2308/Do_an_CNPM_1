// File: Models/Entities/Guest.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("guests")]
    public class Guest
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        [Column("full_name")]
        public string FullName { get; set; } = null!;

        [MaxLength(128)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(32)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(32)]
        [Column("id_number")]
        public string? IdNumber { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation: 1 Guest có nhiều Bookings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
