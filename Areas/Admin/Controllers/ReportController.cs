using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ManagerOrAdmin")]
public class ReportController : Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Dashboard - Tổng quan
    public async Task<IActionResult> Dashboard()
    {
        var summary = await _reportService.GetDashboardSummaryAsync();
        return View(summary);
    }

    // Revenue Report - Báo cáo doanh thu
    public async Task<IActionResult> Revenue(DateTime? fromDate, DateTime? toDate, string groupBy = "Day")
    {
        var from = fromDate ?? DateTime.Now.AddMonths(-1);
        var to = toDate ?? DateTime.Now;

        if (from > to)
        {
            ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
            from = to.AddMonths(-1);
        }

        var report = await _reportService.GetRevenueReportAsync(from, to, groupBy);
        return View(report);
    }

    // Occupancy Report - Báo cáo lấp đầy phòng
    public async Task<IActionResult> Occupancy(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Now.AddMonths(-1);
        var to = toDate ?? DateTime.Now;

        if (from > to)
        {
            ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
            from = to.AddMonths(-1);
        }

        var report = await _reportService.GetOccupancyReportAsync(from, to);
        return View(report);
    }

    // Booking Status Report - Báo cáo trạng thái booking
    public async Task<IActionResult> BookingStatus(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Now.AddMonths(-1);
        var to = toDate ?? DateTime.Now;

        if (from > to)
        {
            ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
            from = to.AddMonths(-1);
        }

        var report = await _reportService.GetBookingStatusReportAsync(from, to);
        return View(report);
    }

    // Top Guests - Top khách hàng
    public async Task<IActionResult> TopGuests(DateTime? fromDate, DateTime? toDate, int top = 10)
    {
        var guests = await _reportService.GetTopGuestsAsync(fromDate, toDate, top);
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.Top = top;
        return View(guests);
    }

    // Export Revenue Report (Optional - could export to Excel/PDF)
    [HttpGet]
    public async Task<IActionResult> ExportRevenue(DateTime fromDate, DateTime toDate, string groupBy = "Day", string format = "csv")
    {
        var report = await _reportService.GetRevenueReportAsync(fromDate, toDate, groupBy);

        if (format.ToLower() == "csv")
        {
            var csv = "Date,Revenue,Bookings\n";
            foreach (var item in report.Data)
            {
                csv += $"{item.Label},{item.Revenue},{item.BookingCount}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"revenue_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
        }

        return BadRequest("Unsupported format");
    }

    // Export Occupancy Report
    [HttpGet]
    public async Task<IActionResult> ExportOccupancy(DateTime fromDate, DateTime toDate, string format = "csv")
    {
        var report = await _reportService.GetOccupancyReportAsync(fromDate, toDate);

        if (format.ToLower() == "csv")
        {
            var csv = "Date,Total Rooms,Occupied Rooms,Occupancy Rate\n";
            foreach (var item in report.Data)
            {
                csv += $"{item.Label},{item.TotalRooms},{item.OccupiedRooms},{item.OccupancyRate}%\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"occupancy_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
        }

        return BadRequest("Unsupported format");
    }
}
