// File: Models/Entities/Booking.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("bookings")]
    public class Booking
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("hotel_id")]
        public long HotelId { get; set; }

        [Required]
        [Column("guest_id")]
        public long GuestId { get; set; }

        [Required]
        [Column("check_in_date")]
        public DateTime CheckInDate { get; set; }

        [Required]
        [Column("check_out_date")]
        public DateTime CheckOutDate { get; set; }

        // ENUM('Pending','Confirmed','CheckedIn','CheckedOut','Cancelled')
        [Required]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Required]
        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        // ENUM('Unpaid','Paid','Refunded','Failed')
        [Required]
        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Column("rateplan_snapshot_json")]
        public string? RatePlanSnapshotJson { get; set; }

        [Column("promotion_id")]
        public long? PromotionId { get; set; }

        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        [Column("checkin_actual_date")]
        public DateTime? CheckInActualDate { get; set; }

        [Column("checkout_actual_date")]
        public DateTime? CheckOutActualDate { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(HotelId))]
        public virtual Hotel Hotel { get; set; } = null!;

        [ForeignKey(nameof(GuestId))]
        public virtual Guest Guest { get; set; } = null!;

        [ForeignKey(nameof(PromotionId))]
        public virtual Promotion? Promotion { get; set; }

        public virtual ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Quan hệ 1-1: mỗi Booking có 1 Invoice
        public virtual Invoice? Invoice { get; set; }
    }
}
