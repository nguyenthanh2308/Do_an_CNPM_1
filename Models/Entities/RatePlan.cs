// File: Models/Entities/RatePlan.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("rateplans")]
    public class RatePlan
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("room_type_id")]
        public long RoomTypeId { get; set; }

        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = null!;

        // ENUM('Flexible','NonRefundable')
        [Required]
        [Column("type")]
        public string Type { get; set; } = null!;

        [Column("free_cancel_until_hours")]
        public int? FreeCancelUntilHours { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; }

        // JSON – bạn có thể map string và xử lý thủ công
        [Column("weekend_rule_json")]
        public string? WeekendRuleJson { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(RoomTypeId))]
        public virtual RoomType RoomType { get; set; } = null!;

        // Navigation property Bookings đã bị xóa vì không có FK relationship
        // Thông tin rate plan được lưu dưới dạng snapshot trong Booking.RatePlanSnapshotJson
    }
}
