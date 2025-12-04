namespace HotelManagementSystem.Models.ViewModels.Invoice;

public class InvoiceViewModel
{
    // Basic Properties
    public ulong Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public ulong BookingId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Cancelled

    // Booking Information
    public string BookingCode { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Nights { get; set; }

    // Guest Information
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string? GuestPhone { get; set; }
    public string? GuestAddress { get; set; }

    // Room Information
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;

    // Financial Breakdown
    public decimal RoomCharge { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal TotalAmount { get; set; }

    // Additional Info
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Display Helpers
    public string StatusDisplay => Status switch
    {
        "Draft" => "Nháp",
        "Issued" => "Đã phát hành",
        "Paid" => "Đã thanh toán",
        "Cancelled" => "Đã hủy",
        _ => Status
    };

    public string StatusBadgeClass => Status switch
    {
        "Draft" => "bg-secondary",
        "Issued" => "bg-info",
        "Paid" => "bg-success",
        "Cancelled" => "bg-danger",
        _ => "bg-secondary"
    };

    // Business Logic Properties
    public bool IsPaid => Status == "Paid";
    public bool IsIssued => Status == "Issued";
    public bool IsDraft => Status == "Draft";
    public bool IsCancelled => Status == "Cancelled";
    public bool CanDownload => Status == "Issued" || Status == "Paid";
    public bool CanEdit => Status == "Draft";
    public bool CanCancel => Status == "Draft" || Status == "Issued";
    public bool CanMarkAsPaid => Status == "Issued";

    // Formatted Properties
    public string FormattedAmount => Amount.ToString("N0") + " đ";
    public string FormattedIssuedDate => IssuedAt.ToString("dd/MM/yyyy");
    public string FormattedCheckInOut => $"{CheckInDate:dd/MM/yyyy} - {CheckOutDate:dd/MM/yyyy}";
}
