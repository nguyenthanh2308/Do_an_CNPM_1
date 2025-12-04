// File: Models/Entities/BookingRoom.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("booking_rooms")]
    public class BookingRoom
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("booking_id")]
        public long BookingId { get; set; }

        [Column("room_id")]
        public long? RoomId { get; set; }

        [Required]
        [Column("price_per_night")]
        public decimal PricePerNight { get; set; }

        [Required]
        [Column("nights")]
        public int Nights { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }
    }
}
