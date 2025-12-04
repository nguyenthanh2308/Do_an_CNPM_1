// File: Models/Entities/Invoice.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("invoices")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("booking_id")]
        public long BookingId { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("number")]
        public string Number { get; set; } = null!;

        // Alias for Number (for compatibility with service layer)
        [NotMapped]
        public string InvoiceNumber
        {
            get => Number;
            set => Number = value;
        }

        [Required]
        [Column("amount", TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column("issued_at")]
        public DateTime IssuedAt { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Cancelled

        [MaxLength(255)]
        [Column("pdf_url")]
        public string? PdfUrl { get; set; }

        [Column("notes", TypeName = "text")]
        public string? Notes { get; set; }

        [MaxLength(50)]
        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // 1-1 với Booking: booking_id có UNIQUE
        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;
    }
}
