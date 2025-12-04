namespace HotelManagementSystem.Models.ViewModels.Housekeeping;

public class HousekeepingTaskViewModel
{
    // Basic Properties
    public ulong Id { get; set; }
    public ulong RoomId { get; set; }
    public ulong? AssignedUserId { get; set; }
    public string TaskType { get; set; } = "Cleaning"; // Cleaning, Maintenance, Inspection, CheckOut
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Room Information
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string RoomStatus { get; set; } = "Available"; // From Room entity

    // Assigned User Information
    public string? AssignedUserName { get; set; }
    public string? AssignedUserRole { get; set; }

    // Related Booking (if CheckOut task)
    public ulong? BookingId { get; set; }
    public string? BookingCode { get; set; }

    // Display Helpers
    public string TaskTypeDisplay => TaskType switch
    {
        "Cleaning" => "Dọn phòng thường",
        "Maintenance" => "Bảo trì/Sửa chữa",
        "Inspection" => "Kiểm tra",
        "CheckOut" => "Dọn sau checkout",
        _ => TaskType
    };

    public string StatusDisplay => Status switch
    {
        "Pending" => "Chờ xử lý",
        "InProgress" => "Đang làm",
        "Completed" => "Hoàn thành",
        "Cancelled" => "Đã hủy",
        _ => Status
    };

    public string PriorityDisplay => Priority switch
    {
        "Low" => "Thấp",
        "Normal" => "Bình thường",
        "High" => "Cao",
        "Urgent" => "Khẩn cấp",
        _ => Priority
    };

    public string StatusBadgeClass => Status switch
    {
        "Pending" => "bg-warning",
        "InProgress" => "bg-info",
        "Completed" => "bg-success",
        "Cancelled" => "bg-danger",
        _ => "bg-secondary"
    };

    public string PriorityBadgeClass => Priority switch
    {
        "Low" => "bg-secondary",
        "Normal" => "bg-primary",
        "High" => "bg-warning",
        "Urgent" => "bg-danger",
        _ => "bg-secondary"
    };

    public string TaskTypeBadgeClass => TaskType switch
    {
        "Cleaning" => "bg-info",
        "Maintenance" => "bg-warning",
        "Inspection" => "bg-secondary",
        "CheckOut" => "bg-danger",
        _ => "bg-primary"
    };

    // Business Logic Properties
    public bool IsPending => Status == "Pending";
    public bool IsInProgress => Status == "InProgress";
    public bool IsCompleted => Status == "Completed";
    public bool IsCancelled => Status == "Cancelled";
    
    public bool CanStart => Status == "Pending" && AssignedUserId.HasValue;
    public bool CanComplete => Status == "InProgress";
    public bool CanCancel => Status == "Pending" || Status == "InProgress";
    public bool CanReassign => Status == "Pending";
    
    public bool IsOverdue => Status == "Pending" && ScheduledAt < DateTime.Now;
    public bool IsUrgent => Priority == "Urgent";
    public bool IsAssigned => AssignedUserId.HasValue;

    // Time Tracking
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - ScheduledAt : null;
    public string DurationDisplay => Duration.HasValue 
        ? $"{Duration.Value.Hours}h {Duration.Value.Minutes}m" 
        : "-";

    public string ScheduledTimeDisplay => ScheduledAt.ToString("dd/MM/yyyy HH:mm");
    public string CompletedTimeDisplay => CompletedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
}
