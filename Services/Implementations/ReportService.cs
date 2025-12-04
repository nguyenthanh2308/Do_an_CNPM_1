using HotelManagementSystem.Data;
using HotelManagementSystem.Models.ViewModels.Report;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations;

public class ReportService : IReportService
{
    private readonly HotelDbContext _context;

    public ReportService(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime fromDate, DateTime toDate, string groupBy = "Day")
    {
        var bookings = await _context.Bookings
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r.RoomType)
            .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate
                && (b.Status == "Confirmed" || b.Status == "CheckedIn" || b.Status == "CheckedOut"))
            .ToListAsync();

        var totalRevenue = bookings.Sum(b => b.TotalAmount - b.DiscountAmount);
        var totalBookings = bookings.Count;

        var data = new List<RevenueDataPoint>();
        
        switch (groupBy.ToLower())
        {
            case "year":
                data = bookings
                    .GroupBy(b => new { Year = b.CreatedAt.Year })
                    .Select(g => new RevenueDataPoint
                    {
                        Label = g.Key.Year.ToString(),
                        Date = new DateTime(g.Key.Year, 1, 1),
                        Revenue = g.Sum(b => b.TotalAmount - b.DiscountAmount),
                        BookingCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                break;

            case "month":
                data = bookings
                    .GroupBy(b => new { Year = b.CreatedAt.Year, Month = b.CreatedAt.Month })
                    .Select(g => new RevenueDataPoint
                    {
                        Label = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Revenue = g.Sum(b => b.TotalAmount - b.DiscountAmount),
                        BookingCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                break;

            default: // Day
                data = bookings
                    .GroupBy(b => b.CreatedAt.Date)
                    .Select(g => new RevenueDataPoint
                    {
                        Label = g.Key.ToString("dd/MM/yyyy"),
                        Date = g.Key,
                        Revenue = g.Sum(b => b.TotalAmount - b.DiscountAmount),
                        BookingCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                break;
        }

        // Top room types by revenue
        var topRoomTypes = bookings
            .SelectMany(b => b.BookingRooms.Select(br => new { b, br }))
            .GroupBy(x => x.br.Room.RoomType.Name)
            .Select(g => (RoomType: g.Key, Revenue: g.Sum(x => x.b.TotalAmount - x.b.DiscountAmount)))
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        return new RevenueReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            GroupBy = groupBy,
            TotalRevenue = totalRevenue,
            TotalBookings = totalBookings,
            AverageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0,
            Data = data,
            TopRoomTypes = topRoomTypes
        };
    }

    public async Task<OccupancyReportViewModel> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate)
    {
        var totalRooms = await _context.Rooms.CountAsync();
        var days = (int)(toDate - fromDate).TotalDays + 1;
        var totalRoomNights = totalRooms * days;

        // Calculate occupied room nights
        var bookings = await _context.Bookings
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .Where(b => b.Status != "Cancelled" && b.Status != "Pending"
                && b.CheckInDate <= toDate && b.CheckOutDate >= fromDate)
            .ToListAsync();

        int occupiedRoomNights = 0;
        foreach (var booking in bookings)
        {
            var start = booking.CheckInDate > fromDate ? booking.CheckInDate : fromDate;
            var end = booking.CheckOutDate < toDate ? booking.CheckOutDate : toDate;
            var nights = (int)(end - start).TotalDays;
            if (nights > 0)
                occupiedRoomNights += nights;
        }

        double occupancyRate = totalRoomNights > 0 ? (double)occupiedRoomNights / totalRoomNights * 100 : 0;

        // Daily occupancy data
        var data = new List<OccupancyDataPoint>();
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            var occupiedCount = bookings.Count(b => b.CheckInDate <= date && b.CheckOutDate > date);
            var rate = totalRooms > 0 ? (double)occupiedCount / totalRooms * 100 : 0;

            data.Add(new OccupancyDataPoint
            {
                Label = date.ToString("dd/MM"),
                Date = date,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedCount,
                OccupancyRate = Math.Round(rate, 2)
            });
        }

        // By room type
        var roomTypes = await _context.RoomTypes.Include(rt => rt.Rooms).ToListAsync();
        var byRoomType = new List<(string RoomType, double OccupancyRate, int TotalRooms)>();

        foreach (var rt in roomTypes)
        {
            var rtRooms = rt.Rooms.Count;
            var rtRoomNights = rtRooms * days;
            var rtRoomIds = rt.Rooms.Select(r => r.Id).ToList();
            var rtBookings = bookings.Where(b => b.BookingRooms.Any(br => br.RoomId.HasValue && rtRoomIds.Contains(br.RoomId.Value))).ToList();

            int rtOccupiedNights = 0;
            foreach (var booking in rtBookings)
            {
                var start = booking.CheckInDate > fromDate ? booking.CheckInDate : fromDate;
                var end = booking.CheckOutDate < toDate ? booking.CheckOutDate : toDate;
                var nights = (int)(end - start).TotalDays;
                if (nights > 0)
                    rtOccupiedNights += nights;
            }

            double rtRate = rtRoomNights > 0 ? (double)rtOccupiedNights / rtRoomNights * 100 : 0;
            byRoomType.Add((rt.Name, Math.Round(rtRate, 2), rtRooms));
        }

        return new OccupancyReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRooms = totalRooms,
            TotalRoomNights = totalRoomNights,
            OccupiedRoomNights = occupiedRoomNights,
            OccupancyRate = Math.Round(occupancyRate, 2),
            Data = data,
            ByRoomType = byRoomType
        };
    }

    public async Task<BookingStatusReportViewModel> GetBookingStatusReportAsync(DateTime fromDate, DateTime toDate)
    {
        var bookings = await _context.Bookings
            .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
            .ToListAsync();

        var total = bookings.Count;
        var confirmed = bookings.Count(b => b.Status == "Confirmed");
        var checkedIn = bookings.Count(b => b.Status == "CheckedIn");
        var checkedOut = bookings.Count(b => b.Status == "CheckedOut");
        var cancelled = bookings.Count(b => b.Status == "Cancelled");
        var pending = bookings.Count(b => b.Status == "Pending");

        var cancellationRate = total > 0 ? (double)cancelled / total * 100 : 0;
        var confirmationRate = total > 0 ? (double)(confirmed + checkedIn + checkedOut) / total * 100 : 0;

        var statusBreakdown = new List<(string Status, int Count, double Percentage)>
        {
            ("Pending", pending, total > 0 ? (double)pending / total * 100 : 0),
            ("Confirmed", confirmed, total > 0 ? (double)confirmed / total * 100 : 0),
            ("CheckedIn", checkedIn, total > 0 ? (double)checkedIn / total * 100 : 0),
            ("CheckedOut", checkedOut, total > 0 ? (double)checkedOut / total * 100 : 0),
            ("Cancelled", cancelled, total > 0 ? (double)cancelled / total * 100 : 0)
        };

        return new BookingStatusReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalBookings = total,
            ConfirmedBookings = confirmed,
            CheckedInBookings = checkedIn,
            CheckedOutBookings = checkedOut,
            CancelledBookings = cancelled,
            PendingBookings = pending,
            CancellationRate = Math.Round(cancellationRate, 2),
            ConfirmationRate = Math.Round(confirmationRate, 2),
            StatusBreakdown = statusBreakdown
        };
    }

    public async Task<DashboardSummaryViewModel> GetDashboardSummaryAsync()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);
        var last7Days = today.AddDays(-7);

        // Today's stats
        var todayBookings = await _context.Bookings
            .Where(b => b.CreatedAt.Date == today && b.Status != "Cancelled")
            .ToListAsync();
        var todayRevenue = todayBookings.Sum(b => b.TotalAmount - b.DiscountAmount);
        var todayCheckIns = await _context.Bookings.CountAsync(b => b.CheckInDate == today);
        var todayCheckOuts = await _context.Bookings.CountAsync(b => b.CheckOutDate == today);

        // Month stats
        var monthBookings = await _context.Bookings
            .Where(b => b.CreatedAt >= monthStart && b.Status != "Cancelled")
            .ToListAsync();
        var monthRevenue = monthBookings.Sum(b => b.TotalAmount - b.DiscountAmount);

        // Year stats
        var yearBookings = await _context.Bookings
            .Where(b => b.CreatedAt >= yearStart && b.Status != "Cancelled")
            .ToListAsync();
        var yearRevenue = yearBookings.Sum(b => b.TotalAmount - b.DiscountAmount);

        // Current status
        var totalRooms = await _context.Rooms.CountAsync();
        var currentOccupied = await _context.Bookings.CountAsync(b => 
            b.Status == "CheckedIn" && b.CheckInDate <= today && b.CheckOutDate > today);
        var pendingTasks = await _context.HousekeepingTasks.CountAsync(t => t.Status == "Pending");
        var unpaidInvoices = await _context.Invoices.CountAsync(i => i.Status == "Issued" || i.Status == "Overdue");

        // Month occupancy
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var monthOccupancy = await GetOccupancyReportAsync(monthStart, monthStart.AddDays(daysInMonth - 1));

        // Last 7 days revenue
        var last7DaysRevenue = new List<RevenueDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayBookings = await _context.Bookings
                .Where(b => b.CreatedAt.Date == date && b.Status != "Cancelled")
                .ToListAsync();
            var revenue = dayBookings.Sum(b => b.TotalAmount - b.DiscountAmount);

            last7DaysRevenue.Add(new RevenueDataPoint
            {
                Label = date.ToString("dd/MM"),
                Date = date,
                Revenue = revenue,
                BookingCount = dayBookings.Count
            });
        }

        // Last 7 days occupancy
        var last7DaysOccupancy = new List<OccupancyDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var occupied = await _context.Bookings.CountAsync(b =>
                b.Status == "CheckedIn" && b.CheckInDate <= date && b.CheckOutDate > date);
            var rate = totalRooms > 0 ? (double)occupied / totalRooms * 100 : 0;

            last7DaysOccupancy.Add(new OccupancyDataPoint
            {
                Label = date.ToString("dd/MM"),
                Date = date,
                TotalRooms = totalRooms,
                OccupiedRooms = occupied,
                OccupancyRate = Math.Round(rate, 2)
            });
        }

        // Trends (compare with last period)
        var lastMonth = monthStart.AddMonths(-1);
        var lastMonthBookings = await _context.Bookings
            .Where(b => b.CreatedAt >= lastMonth && b.CreatedAt < monthStart && b.Status != "Cancelled")
            .ToListAsync();
        var lastMonthRevenue = lastMonthBookings.Sum(b => b.TotalAmount - b.DiscountAmount);

        double revenueTrend = lastMonthRevenue > 0 ? (double)((monthRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0;
        double bookingTrend = lastMonthBookings.Count > 0 ? (double)((monthBookings.Count - lastMonthBookings.Count) / (double)lastMonthBookings.Count * 100) : 0;

        return new DashboardSummaryViewModel
        {
            TodayRevenue = todayRevenue,
            TodayBookings = todayBookings.Count,
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            MonthRevenue = monthRevenue,
            MonthBookings = monthBookings.Count,
            MonthOccupancyRate = monthOccupancy.OccupancyRate,
            YearRevenue = yearRevenue,
            YearBookings = yearBookings.Count,
            CurrentOccupiedRooms = currentOccupied,
            TotalRooms = totalRooms,
            PendingTasks = pendingTasks,
            UnpaidInvoices = unpaidInvoices,
            RevenueTrend = Math.Round(revenueTrend, 2),
            BookingTrend = Math.Round(bookingTrend, 2),
            OccupancyTrend = 0, // Could calculate if needed
            Last7DaysRevenue = last7DaysRevenue,
            Last7DaysOccupancy = last7DaysOccupancy
        };
    }

    public async Task<List<(ulong GuestId, string GuestName, decimal TotalSpent, int BookingCount)>> GetTopGuestsAsync(
        DateTime? fromDate = null, DateTime? toDate = null, int top = 10)
    {
        var query = _context.Bookings
            .Include(b => b.Guest)
            .Where(b => b.Status != "Cancelled");

        if (fromDate.HasValue)
            query = query.Where(b => b.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(b => b.CreatedAt <= toDate.Value);

        var result = await query
            .GroupBy(b => new { b.GuestId, b.Guest.FullName })
            .Select(g => new
            {
                GuestId = g.Key.GuestId,
                GuestName = g.Key.FullName,
                TotalSpent = g.Sum(b => b.TotalAmount - b.DiscountAmount),
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(top)
            .ToListAsync();

        return result.Select(x => ((ulong)x.GuestId, x.GuestName, x.TotalSpent, x.BookingCount)).ToList();
    }

    public async Task<List<(string RoomType, decimal Revenue, int BookingCount)>> GetRevenueByRoomTypeAsync(
        DateTime fromDate, DateTime toDate)
    {
        var bookings = await _context.Bookings
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate && b.Status != "Cancelled")
            .ToListAsync();

        var result = bookings
            .SelectMany(b => b.BookingRooms.Select(br => new { b, br }))
            .Where(x => x.br.Room != null)
            .GroupBy(x => x.br.Room!.RoomType.Name)
            .Select(g => (
                RoomType: g.Key,
                Revenue: g.Sum(x => x.b.TotalAmount - x.b.DiscountAmount),
                BookingCount: g.Select(x => x.b.Id).Distinct().Count()
            ))
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return result;
    }
}
