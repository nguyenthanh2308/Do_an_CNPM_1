// File: Models/Entities/Room.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("hotel_id")]
        public long HotelId { get; set; }

        [Required]
        [Column("room_type_id")]
        public long RoomTypeId { get; set; }

        [Required]
        [MaxLength(16)]
        [Column("number")]
        public string Number { get; set; } = null!;

        [Column("floor")]
        public short? Floor { get; set; }

        // ENUM('Vacant','Occupied','Cleaning','Maintenance')
        [Required]
        [Column("status")]
        public string Status { get; set; } = "Vacant";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(HotelId))]
        public virtual Hotel Hotel { get; set; } = null!;

        [ForeignKey(nameof(RoomTypeId))]
        public virtual RoomType RoomType { get; set; } = null!;

        public virtual ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
        public virtual ICollection<HousekeepingTask> HousekeepingTasks { get; set; } = new List<HousekeepingTask>();
    }
}
