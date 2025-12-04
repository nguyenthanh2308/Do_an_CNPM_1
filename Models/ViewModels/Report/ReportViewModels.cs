namespace HotelManagementSystem.Models.ViewModels.Report;

// Revenue Report
public class RevenueReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string GroupBy { get; set; } = "Day"; // Day, Month, Year
    
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    
    public List<RevenueDataPoint> Data { get; set; } = new();
    
    // Top revenue sources
    public List<(string RoomType, decimal Revenue)> TopRoomTypes { get; set; } = new();
}

public class RevenueDataPoint
{
    public string Label { get; set; } = string.Empty; // Date label
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

// Occupancy Report
public class OccupancyReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public int TotalRooms { get; set; }
    public int TotalRoomNights { get; set; } // Total capacity (rooms * nights)
    public int OccupiedRoomNights { get; set; }
    public double OccupancyRate { get; set; } // Percentage
    
    public List<OccupancyDataPoint> Data { get; set; } = new();
    
    // By room type
    public List<(string RoomType, double OccupancyRate, int TotalRooms)> ByRoomType { get; set; } = new();
}

public class OccupancyDataPoint
{
    public string Label { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public double OccupancyRate { get; set; }
}

// Booking Status Report
public class BookingStatusReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CheckedOutBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    
    public double CancellationRate { get; set; }
    public double ConfirmationRate { get; set; }
    
    public List<(string Status, int Count, double Percentage)> StatusBreakdown { get; set; } = new();
}

// Dashboard Summary
public class DashboardSummaryViewModel
{
    // Today's stats
    public decimal TodayRevenue { get; set; }
    public int TodayBookings { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    
    // Month stats
    public decimal MonthRevenue { get; set; }
    public int MonthBookings { get; set; }
    public double MonthOccupancyRate { get; set; }
    
    // Year stats
    public decimal YearRevenue { get; set; }
    public int YearBookings { get; set; }
    
    // Current status
    public int CurrentOccupiedRooms { get; set; }
    public int TotalRooms { get; set; }
    public int PendingTasks { get; set; }
    public int UnpaidInvoices { get; set; }
    
    // Trends (vs last period)
    public double RevenueTrend { get; set; } // Percentage change
    public double BookingTrend { get; set; }
    public double OccupancyTrend { get; set; }
    
    // Quick charts data
    public List<RevenueDataPoint> Last7DaysRevenue { get; set; } = new();
    public List<OccupancyDataPoint> Last7DaysOccupancy { get; set; } = new();
}
