// File: Models/Entities/Hotel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("hotels")]
    public class Hotel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("timezone")]
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation: 1 Hotel có nhiều RoomTypes, Rooms, Bookings
        public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
