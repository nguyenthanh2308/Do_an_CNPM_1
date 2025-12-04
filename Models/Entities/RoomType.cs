// File: Models/Entities/RoomType.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("room_types")]
    public class RoomType
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("hotel_id")]
        public long HotelId { get; set; }

        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("capacity")]
        public byte Capacity { get; set; }

        [Required]
        [Column("base_price")]
        public decimal BasePrice { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Column("default_image_url")]
        public string? DefaultImageUrl { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(HotelId))]
        public virtual Hotel Hotel { get; set; } = null!;

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<RatePlan> RatePlans { get; set; } = new List<RatePlan>();
        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; }
            = new List<RoomTypeAmenity>();
    }
}
