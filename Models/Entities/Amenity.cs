// File: Models/Entities/Amenity.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("amenities")]
    public class Amenity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("name")]
        public string Name { get; set; } = null!;

        // Many-to-many: RoomTypes liên kết qua RoomTypeAmenity
        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; }
            = new List<RoomTypeAmenity>();
    }
}
