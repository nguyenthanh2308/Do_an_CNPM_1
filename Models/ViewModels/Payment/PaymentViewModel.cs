// File: Models/ViewModels/Payment/PaymentViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Payment
{
    public class PaymentViewModel
    {
        public long Id { get; set; }
        
        [Display(Name = "Mã thanh toán")]
        public string PaymentCode => $"PAY{Id:D6}";
        
        // Booking Info
        public long BookingId { get; set; }
        
        [Display(Name = "Mã booking")]
        public string BookingCode { get; set; } = string.Empty;
        
        [Display(Name = "Khách hàng")]
        public string GuestName { get; set; } = string.Empty;
        
        [Display(Name = "Phòng")]
        public string RoomNumber { get; set; } = string.Empty;
        
        // Payment Info
        [Display(Name = "Phương thức")]
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string Method { get; set; } = "Mock";
        
        [Display(Name = "Số tiền")]
        [Required(ErrorMessage = "Số tiền không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal Amount { get; set; }
        
        [Display(Name = "Mã giao dịch")]
        public string? TxnCode { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Unpaid";
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }
        
        // Display Properties
        public string MethodDisplay => Method switch
        {
            "Mock" => "Thanh toán giả lập",
            "PayAtProperty" => "Thanh toán tại khách sạn",
            _ => Method
        };
        
        public string StatusDisplay => Status switch
        {
            "Unpaid" => "Chưa thanh toán",
            "Paid" => "Đã thanh toán",
            "Refunded" => "Đã hoàn tiền",
            "Failed" => "Thất bại",
            _ => Status
        };
        
        public string StatusBadgeClass => Status switch
        {
            "Unpaid" => "bg-warning",
            "Paid" => "bg-success",
            "Refunded" => "bg-info",
            "Failed" => "bg-danger",
            _ => "bg-secondary"
        };
        
        public string MethodBadgeClass => Method switch
        {
            "Mock" => "bg-primary",
            "PayAtProperty" => "bg-secondary",
            _ => "bg-secondary"
        };
        
        // Business Logic
        public bool IsPaid => Status == "Paid";
        public bool CanRefund => Status == "Paid";
        public bool IsFailed => Status == "Failed";
        public bool IsRefunded => Status == "Refunded";
    }
}
