using System;
using System.Collections.Generic;
using HotelManagementSystem.Models.ViewModels.Payment;

namespace HotelManagementSystem.Models.ViewModels.Invoice
{
    public class InvoiceDetailViewModel
    {
        public long InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Notes { get; set; }

        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // Booking Info
        public long BookingId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }

        // Financial Breakdown
        public decimal RoomPricePerNight { get; set; }
        public decimal RoomTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? PromotionCode { get; set; }
        
        // Calculated fields
        public decimal SubTotal => RoomTotal - DiscountAmount;
        public decimal TaxAmount => SubTotal * 0.1m; // 10% VAT
        public decimal ServiceCharge => SubTotal * 0.05m; // 5% Service Charge
        public decimal TotalAmount { get; set; } // Should match Invoice.Amount

        // Payment Info
        public PaymentSummaryViewModel PaymentInfo { get; set; } = new();

        public string StatusDisplay => Status switch
        {
            "Draft" => "Nháp",
            "Issued" => "Đã xuất",
            "Paid" => "Đã thanh toán",
            "Cancelled" => "Đã hủy",
            _ => Status
        };
    }
}
