// File: Models/Entities/RoomTypeAmenity.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("room_type_amenities")]
    public class RoomTypeAmenity
    {
        [Column("room_type_id")]
        public long RoomTypeId { get; set; }

        [Column("amenity_id")]
        public long AmenityId { get; set; }

        [ForeignKey(nameof(RoomTypeId))]
        public virtual RoomType RoomType { get; set; } = null!;

        [ForeignKey(nameof(AmenityId))]
        public virtual Amenity Amenity { get; set; } = null!;

        // ❗ PK là composite (room_type_id, amenity_id) → cần cấu hình trong OnModelCreating:
        // modelBuilder.Entity<RoomTypeAmenity>()
        //   .HasKey(x => new { x.RoomTypeId, x.AmenityId });
    }
}
