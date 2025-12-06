using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Payment
{
    public class PaymentSummaryViewModel
    {
        public long BookingId { get; set; }
        
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Đã thanh toán")]
        public decimal PaidAmount { get; set; }
        
        [Display(Name = "Còn lại")]
        public decimal RemainingAmount { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string PaymentStatus { get; set; } = "Unpaid";
        
        [Display(Name = "Phương thức")]
        public string? PaymentMethod { get; set; }
        
        [Display(Name = "Ngày thanh toán")]
        public DateTime? LastPaymentDate { get; set; }

        public bool IsFullyPaid => RemainingAmount <= 0;
        
        public string StatusDisplay => PaymentStatus switch
        {
            "Unpaid" => "Chưa thanh toán",
            "Paid" => "Đã thanh toán",
            "Refunded" => "Đã hoàn tiền",
            "Failed" => "Thất bại",
            _ => PaymentStatus
        };

        public string StatusBadgeClass => PaymentStatus switch
        {
            "Unpaid" => "bg-warning text-dark",
            "Paid" => "bg-success",
            "Refunded" => "bg-info",
            "Failed" => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
