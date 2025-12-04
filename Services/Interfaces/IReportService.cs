using HotelManagementSystem.Models.ViewModels.Report;

namespace HotelManagementSystem.Services.Interfaces;

public interface IReportService
{
    /// <summary>
    /// Báo cáo doanh thu theo khoảng thời gian
    /// </summary>
    Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "Day");

    /// <summary>
    /// Báo cáo tỷ lệ lấp đầy phòng
    /// </summary>
    Task<OccupancyReportViewModel> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Báo cáo booking theo trạng thái
    /// </summary>
    Task<BookingStatusReportViewModel> GetBookingStatusReportAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Dashboard tổng quan cho Admin/Manager
    /// </summary>
    Task<DashboardSummaryViewModel> GetDashboardSummaryAsync();

    /// <summary>
    /// Lấy top khách hàng (theo doanh thu)
    /// </summary>
    Task<List<(ulong GuestId, string GuestName, decimal TotalSpent, int BookingCount)>> GetTopGuestsAsync(DateTime? fromDate = null, DateTime? toDate = null, int top = 10);

    /// <summary>
    /// Lấy doanh thu theo phòng/loại phòng
    /// </summary>
    Task<List<(string RoomType, decimal Revenue, int BookingCount)>> GetRevenueByRoomTypeAsync(DateTime fromDate, DateTime toDate);
}
