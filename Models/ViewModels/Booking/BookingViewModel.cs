// File: Models/ViewModels/Booking/BookingViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Booking
{
    public class BookingViewModel
    {
        // Basic Booking Info
        public long Id { get; set; }
        
        [Display(Name = "Mã đặt phòng")]
        public string BookingCode => $"BK{Id:D6}";
        
        public long HotelId { get; set; }
        
        [Display(Name = "Khách sạn")]
        public string HotelName { get; set; } = string.Empty;
        
        public long GuestId { get; set; }
        
        [Display(Name = "Khách hàng")]
        public string GuestName { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        public string? GuestEmail { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string? GuestPhone { get; set; }
        
        // Room Info
        public long RoomId { get; set; }
        
        [Display(Name = "Số phòng")]
        public string RoomNumber { get; set; } = string.Empty;
        
        [Display(Name = "Loại phòng")]
        public string RoomTypeName { get; set; } = string.Empty;
        
        // Date Info
        [Display(Name = "Ngày nhận phòng")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }
        
        [Display(Name = "Ngày trả phòng")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }
        
        [Display(Name = "Số đêm")]
        public int Nights => (CheckOutDate.Date - CheckInDate.Date).Days;
        
        // Status
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Pending";
        
        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; } = "Unpaid";
        
        // Financial Info
        [Display(Name = "Tổng tiền")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Giảm giá")]
        [DataType(DataType.Currency)]
        public decimal DiscountAmount { get; set; }
        
        [Display(Name = "Thành tiền")]
        [DataType(DataType.Currency)]
        public decimal FinalAmount => TotalAmount - DiscountAmount;
        
        // Promotion Info
        public long? PromotionId { get; set; }
        
        [Display(Name = "Mã khuyến mãi")]
        public string? PromotionCode { get; set; }
        
        // Rate Plan Snapshot
        public string? RatePlanSnapshotJson { get; set; }
        public string? RatePlanName { get; set; }
        public string? RatePlanType { get; set; }
        public int? FreeCancelUntilHours { get; set; }
        
        // Timestamps
        [Display(Name = "Ngày đặt")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Ngày hủy")]
        public DateTime? CancelledAt { get; set; }
        
        [Display(Name = "Ngày chỉnh sửa")]
        public DateTime? ModifiedAt { get; set; }
        
        [Display(Name = "Check-in thực tế")]
        public DateTime? CheckInActualDate { get; set; }
        
        [Display(Name = "Check-out thực tế")]
        public DateTime? CheckOutActualDate { get; set; }

        // New Properties for Phase 2 & 3
        public HotelManagementSystem.Models.ViewModels.Payment.PaymentSummaryViewModel PaymentSummary { get; set; } = new();
        
        public string? InvoiceNumber { get; set; }
        public string? InvoiceStatus { get; set; }
        
        // Display Properties
        public string StatusDisplay => Status switch
        {
            "AwaitingPayment" => "Chờ thanh toán",
            "Pending" => "Chờ xác nhận",
            "Confirmed" => "Đã xác nhận",
            "CheckedIn" => "Đã nhận phòng",
            "CheckedOut" => "Đã trả phòng",
            "Cancelled" => "Đã hủy",
            _ => Status
        };
        
        public string StatusBadgeClass => Status switch
        {
            "AwaitingPayment" => "bg-warning",
            "Pending" => "bg-warning",
            "Confirmed" => "bg-info",
            "CheckedIn" => "bg-success",
            "CheckedOut" => "bg-secondary",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
        
        public string PaymentStatusDisplay => PaymentStatus switch
        {
            "Unpaid" => "Chưa thanh toán",
            "Paid" => "Đã thanh toán",
            "Refunded" => "Đã hoàn tiền",
            "Failed" => "Thất bại",
            _ => PaymentStatus
        };
        
        public string PaymentStatusBadgeClass => PaymentStatus switch
        {
            "Unpaid" => "bg-warning",
            "Paid" => "bg-success",
            "Refunded" => "bg-info",
            "Failed" => "bg-danger",
            _ => "bg-secondary"
        };
        
        // Business Logic Properties
        public bool IsCancellable => Status is "Pending" or "Confirmed" && !IsCancelled;
        
        public bool IsModifiable => Status is "Pending" or "Confirmed" && !IsCancelled;
        
        public bool CanCheckIn => Status == "Confirmed" && 
                                   CheckInDate.Date <= DateTime.Today && 
                                   CheckInActualDate == null;
        
        public bool CanCheckOut => Status == "CheckedIn" && CheckOutActualDate == null;
        
        public bool IsCancelled => Status == "Cancelled";
        
        public bool IsActive => Status is "Confirmed" or "CheckedIn";
        
        public bool IsCompleted => Status == "CheckedOut";
        
        // Cancellation Policy Helpers
        public bool CanCancelFree
        {
            get
            {
                if (IsCancelled || Status == "CheckedIn" || Status == "CheckedOut")
                    return false;
                
                if (!FreeCancelUntilHours.HasValue)
                    return false;
                
                var cancelDeadline = CheckInDate.AddHours(-FreeCancelUntilHours.Value);
                return DateTime.Now < cancelDeadline;
            }
        }
        
        public int HoursUntilCheckIn
        {
            get
            {
                var timeUntil = CheckInDate - DateTime.Now;
                return timeUntil.TotalHours > 0 ? (int)Math.Ceiling(timeUntil.TotalHours) : 0;
            }
        }
        
        public decimal RefundAmount
        {
            get
            {
                if (CanCancelFree)
                    return FinalAmount;
                
                if (RatePlanType == "NonRefundable")
                    return 0;
                
                // Flexible but past free cancellation period: 50% refund
                return FinalAmount * 0.5m;
            }
        }
        
        public string CancellationPolicyInfo
        {
            get
            {
                if (RatePlanType == "NonRefundable")
                    return "Không hoàn tiền khi hủy";
                
                if (FreeCancelUntilHours.HasValue)
                    return $"Miễn phí hủy trước {FreeCancelUntilHours} giờ check-in. Sau đó hoàn 50% tổng tiền.";
                
                return "Chính sách hủy linh hoạt";
            }
        }
        
        // Date Range Validation
        public int DaysUntilCheckIn
        {
            get
            {
                var days = (CheckInDate.Date - DateTime.Today).Days;
                return days > 0 ? days : 0;
            }
        }
        
        public bool IsUpcoming => Status is "Pending" or "Confirmed" && CheckInDate.Date > DateTime.Today;
        
        public bool IsToday => CheckInDate.Date == DateTime.Today && Status is "Pending" or "Confirmed";
        
        public bool IsOverdue => CheckInDate.Date < DateTime.Today && Status is "Pending" or "Confirmed";
    }
}
