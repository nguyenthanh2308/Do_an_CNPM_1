namespace HotelManagement.API.Services.Interfaces
{
    public interface IReportService
    {
        // Revenue Reports
        Task<object> GetDailyRevenueReportAsync(DateTime date);
        Task<object> GetMonthlyRevenueReportAsync(int year, int month);
        Task<object> GetYearlyRevenueReportAsync(int year);
        Task<object> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Booking Reports
        Task<object> GetBookingStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<object> GetOccupancyReportAsync(DateTime date);
        Task<object> GetOccupancyTrendAsync(DateTime startDate, DateTime endDate);

        // Room Reports
        Task<object> GetRoomStatusReportAsync();
        Task<object> GetRoomPerformanceReportAsync(DateTime startDate, DateTime endDate);

        // Guest Reports
        Task<object> GetGuestStatisticsAsync();
        Task<object> GetTopGuestsReportAsync(int topCount = 10);

        // Financial Reports
        Task<object> GetPaymentReportAsync(DateTime startDate, DateTime endDate);
        Task<object> GetOutstandingPaymentsReportAsync();

        // Housekeeping Reports
        Task<object> GetHousekeepingTaskReportAsync(DateTime date);
        Task<object> GetHousekeepingPerformanceReportAsync(DateTime startDate, DateTime endDate);

        // Export functionality
        Task<byte[]> ExportReportToPdfAsync(string reportType, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportReportToExcelAsync(string reportType, DateTime startDate, DateTime endDate);
    }
}
