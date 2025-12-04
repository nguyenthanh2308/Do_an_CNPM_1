// File: Models/Entities/Payment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("booking_id")]
        public long BookingId { get; set; }

        // ENUM('Mock','PayAtProperty')
        [Required]
        [Column("method")]
        public string Method { get; set; } = null!;

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [MaxLength(64)]
        [Column("txn_code")]
        public string? TxnCode { get; set; }

        // ENUM('Unpaid','Paid','Refunded','Failed')
        [Required]
        [Column("status")]
        public string Status { get; set; } = "Unpaid";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;
    }
}
