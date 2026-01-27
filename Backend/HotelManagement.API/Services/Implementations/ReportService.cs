using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;

namespace HotelManagement.API.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<object> GetDailyRevenueReportAsync(DateTime date)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(date.Date, date.Date.AddDays(1));
            
            var paidBookings = bookings.Where(b => b.PaymentStatus == "Paid");
            var totalRevenue = paidBookings.Sum(b => b.TotalAmount);
            var bookingCount = paidBookings.Count();

            return new
            {
                Date = date.Date,
                TotalRevenue = totalRevenue,
                BookingCount = bookingCount,
                AverageBookingValue = bookingCount > 0 ? totalRevenue / bookingCount : 0
            };
        }

        public async Task<object> GetMonthlyRevenueReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            
            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(startDate, endDate);
            var paidBookings = bookings.Where(b => b.PaymentStatus == "Paid").ToList();

            var dailyRevenue = paidBookings
                .GroupBy(b => b.CheckInDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new
            {
                Year = year,
                Month = month,
                TotalRevenue = paidBookings.Sum(b => b.TotalAmount),
                TotalBookings = paidBookings.Count,
                DailyBreakdown = dailyRevenue
            };
        }

        public async Task<object> GetYearlyRevenueReportAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);
            
            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(startDate, endDate);
            var paidBookings = bookings.Where(b => b.PaymentStatus == "Paid").ToList();

            var monthlyRevenue = paidBookings
                .GroupBy(b => b.CheckInDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            return new
            {
                Year = year,
                TotalRevenue = paidBookings.Sum(b => b.TotalAmount),
                TotalBookings = paidBookings.Count,
                MonthlyBreakdown = monthlyRevenue
            };
        }

        public async Task<object> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(startDate, endDate);
            var paidBookings = bookings.Where(b => b.PaymentStatus == "Paid").ToList();

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = paidBookings.Sum(b => b.TotalAmount),
                TotalBookings = paidBookings.Count,
                AverageBookingValue = paidBookings.Any() ? paidBookings.Average(b => b.TotalAmount) : 0
            };
        }

        public async Task<object> GetBookingStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(startDate, endDate);

            return new
            {
                TotalBookings = bookings.Count(),
                ConfirmedBookings = bookings.Count(b => b.Status == "Confirmed"),
                PendingBookings = bookings.Count(b => b.Status == "Pending"),
                CheckedInBookings = bookings.Count(b => b.Status == "CheckedIn"),
                CheckedOutBookings = bookings.Count(b => b.Status == "CheckedOut"),
                CancelledBookings = bookings.Count(b => b.Status == "Cancelled"),
                StatusBreakdown = bookings.GroupBy(b => b.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        public async Task<object> GetOccupancyReportAsync(DateTime date)
        {
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();
            var totalRooms = allRooms.Count();

            var occupiedRooms = allRooms.Count(r => r.Status == "Occupied");
            var availableRooms = allRooms.Count(r => r.Status == "Vacant");

            var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

            return new
            {
                Date = date.Date,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                AvailableRooms = availableRooms,
                OccupancyRate = Math.Round(occupancyRate, 2)
            };
        }

        public async Task<object> GetOccupancyTrendAsync(DateTime startDate, DateTime endDate)
        {
            var allRooms = await _unitOfWork.Rooms.GetAllAsync();
            var totalRooms = allRooms.Count();

            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(startDate, endDate);
            
            var days = (int)(endDate - startDate).TotalDays;
            var occupancyTrend = new List<object>();

            for (int i = 0; i <= days; i++)
            {
                var currentDate = startDate.AddDays(i);
                var occupiedCount = bookings.Count(b => 
                    b.Status == "CheckedIn" && 
                    b.CheckInDate <= currentDate && 
                    b.CheckOutDate > currentDate
                );

                occupancyTrend.Add(new
                {
                    Date = currentDate.Date,
                    OccupiedRooms = occupiedCount,
                    OccupancyRate = totalRooms > 0 ? Math.Round((double)occupiedCount / totalRooms * 100, 2) : 0
                });
            }

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRooms = totalRooms,
                OccupancyTrend = occupancyTrend
            };
        }

        public async Task<object> GetRoomStatusReportAsync()
        {
            var rooms = await _unitOfWork.Rooms.GetAllAsync();

            return new
            {
                TotalRooms = rooms.Count(),
                StatusBreakdown = rooms.GroupBy(r => r.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / rooms.Count() * 100, 2)
                    })
                    .ToList()
            };
        }

        public async Task<object> GetRoomPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsByDateRangeAsync(startDate, endDate);
            var allBookingRooms = new List<Models.Entities.BookingRoom>();

            foreach (var booking in bookings)
            {
                var bookingRooms = await _unitOfWork.BookingRooms.GetByBookingIdAsync(booking.Id);
                allBookingRooms.AddRange(bookingRooms);
            }

            var roomPerformance = allBookingRooms
                .GroupBy(br => br.RoomId)
                .Select(g => new
                {
                    RoomId = g.Key,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(br => br.PricePerNight * br.Nights),
                    TotalNights = g.Sum(br => br.Nights)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                RoomPerformance = roomPerformance
            };
        }

        public async Task<object> GetGuestStatisticsAsync()
        {
            var guests = await _unitOfWork.Guests.GetAllAsync();
            var allBookings = await _unitOfWork.Bookings.GetAllAsync();

            return new
            {
                TotalGuests = guests.Count(),
                TotalBookings = allBookings.Count(),
                AverageBookingsPerGuest = guests.Any() ? (double)allBookings.Count() / guests.Count() : 0
            };
        }

        public async Task<object> GetTopGuestsReportAsync(int topCount = 10)
        {
            var allBookings = await _unitOfWork.Bookings.GetAllAsync();
            
            var topGuests = allBookings
                .GroupBy(b => b.GuestId)
                .Select(g => new
                {
                    GuestId = g.Key,
                    BookingCount = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(topCount)
                .ToList();

            return new
            {
                TopGuests = topGuests
            };
        }

        public async Task<object> GetPaymentReportAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();
            var filteredPayments = payments.Where(p => 
                p.CreatedAt >= startDate && 
                p.CreatedAt < endDate.AddDays(1)
            ).ToList();

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalPayments = filteredPayments.Sum(p => p.Amount),
                PaymentCount = filteredPayments.Count,
                PaymentsByMethod = filteredPayments.GroupBy(p => p.Method)
                    .Select(g => new
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        Total = g.Sum(p => p.Amount)
                    })
                    .ToList(),
                PaymentsByStatus = filteredPayments.GroupBy(p => p.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Total = g.Sum(p => p.Amount)
                    })
                    .ToList()
            };
        }

        public async Task<object> GetOutstandingPaymentsReportAsync()
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync();
            var outstandingBookings = new List<object>();

            foreach (var booking in bookings.Where(b => b.PaymentStatus != "Paid"))
            {
                var totalPaid = await _unitOfWork.Payments.GetTotalPaidAsync(booking.Id);
                var outstanding = booking.TotalAmount - totalPaid;

                if (outstanding > 0)
                {
                    outstandingBookings.Add(new
                    {
                        BookingId = booking.Id,
                        GuestId = booking.GuestId,
                        TotalAmount = booking.TotalAmount,
                        PaidAmount = totalPaid,
                        OutstandingAmount = outstanding,
                        CheckInDate = booking.CheckInDate,
                        Status = booking.Status
                    });
                }
            }

            return new
            {
                TotalOutstanding = outstandingBookings.Sum(b => (decimal)b.GetType().GetProperty("OutstandingAmount")!.GetValue(b)!),
                OutstandingBookings = outstandingBookings
            };
        }

        public async Task<object> GetHousekeepingTaskReportAsync(DateTime date)
        {
            var tasks = await _unitOfWork.HousekeepingTasks.GetAllAsync();
            var dateTasks = tasks.Where(t => t.CreatedAt.Date == date.Date).ToList();

            return new
            {
                Date = date.Date,
                TotalTasks = dateTasks.Count,
                PendingTasks = dateTasks.Count(t => t.Status == "Pending"),
                InProgressTasks = dateTasks.Count(t => t.Status == "InProgress"),
                CompletedTasks = dateTasks.Count(t => t.Status == "Completed"),
                TasksByType = dateTasks.GroupBy(t => t.TaskType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        public async Task<object> GetHousekeepingPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            var tasks = await _unitOfWork.HousekeepingTasks.GetAllAsync();
            var filteredTasks = tasks.Where(t => 
                t.CreatedAt >= startDate && 
                t.CreatedAt < endDate.AddDays(1)
            ).ToList();

            var completedTasks = filteredTasks.Where(t => t.Status == "Completed").ToList();

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalTasks = filteredTasks.Count,
                CompletedTasks = completedTasks.Count,
                CompletionRate = filteredTasks.Any() ? Math.Round((double)completedTasks.Count / filteredTasks.Count * 100, 2) : 0,
                PerformanceByUser = filteredTasks.Where(t => t.AssignedToUserId.HasValue)
                    .GroupBy(t => t.AssignedToUserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalTasks = g.Count(),
                        CompletedTasks = g.Count(t => t.Status == "Completed")
                    })
                    .ToList()
            };
        }

        public async Task<byte[]> ExportReportToPdfAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            // TODO: Implement PDF export using a library like iTextSharp or DinkToPdf
            _logger.LogWarning("PDF export not yet implemented");
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportReportToExcelAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            // TODO: Implement Excel export using a library like EPPlus or ClosedXML
            _logger.LogWarning("Excel export not yet implemented");
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }
    }
}
