using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Booking;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist,Customer")] // Staff v� Customer c� quy?n booking
    public class BookingController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly HotelManagementSystem.Data.HotelDbContext _context;

        public BookingController(IRoomService roomService, IBookingService bookingService, HotelManagementSystem.Data.HotelDbContext context)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _context = context;
        }

        // GET: /Booking/Rooms
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Rooms()
        {
            var today = DateTime.Today;
            
            var rooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Where(r => r.Status != "Maintenance")
                .ToListAsync();

            // Get all active bookings (Pending, Confirmed, CheckedIn) that overlap with today
            var activeBookingRoomIds = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => br.Booking.Status != "Cancelled" 
                          && br.Booking.Status != "CheckedOut"
                          && br.Booking.CheckInDate <= today
                          && br.Booking.CheckOutDate > today)
                .Select(br => br.RoomId)
                .Distinct()
                .ToListAsync();

            var viewModels = rooms.Select(r => new HotelManagementSystem.Models.ViewModels.Booking.RoomViewModel
            {
                Id = r.Id,
                Number = r.Number,
                Hotel = new HotelManagementSystem.Models.ViewModels.Hotel.HotelViewModel { Name = r.Hotel.Name },
                RoomType = new HotelManagementSystem.Models.ViewModels.RoomType.RoomTypeViewModel 
                { 
                    Id = r.RoomType.Id,
                    Name = r.RoomType.Name,
                    Description = r.RoomType.Description,
                    BasePrice = r.RoomType.BasePrice,
                    Capacity = r.RoomType.Capacity,
                    DefaultImageUrl = r.RoomType.DefaultImageUrl
                },
                Floor = (int)(r.Floor ?? 0),
                // Set status based on active bookings
                Status = activeBookingRoomIds.Contains(r.Id) ? "Occupied" : (r.Status == "Cleaning" ? "Cleaning" : "Vacant"),
                Price = r.RoomType.BasePrice
            }).ToList();

            return View(viewModels);
        }

        // GET: /Booking/Search
        // GET: /Booking/Search
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(BookingSearchViewModel model)
        {
            // If no date selected (initial load), show ALL rooms
            if (model.CheckInDate == null)
            {
                model.CheckInDate = DateTime.Today.AddDays(1);
                model.CheckOutDate = DateTime.Today.AddDays(2);
                model.NumberOfGuests = 1;

                // Fetch all rooms to display "existing rooms in all states"
                var allRooms = await _context.Rooms
                    .Include(r => r.Hotel)
                    .Include(r => r.RoomType)
                    .ToListAsync();

                model.AvailableRooms = allRooms;

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // T�m ph�ng tr?ng b?ng RoomService
            model.AvailableRooms = await _roomService.SearchAvailableRoomsAsync(
                model.CheckInDate!.Value,
                model.CheckOutDate!.Value,
                model.NumberOfGuests,
                model.HotelId,
                model.RoomTypeId
            );

            return View(model);
        }

        // POST: /Booking/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(
            long roomId,
            long ratePlanId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests)
        {
            // L?y GuestId t? User dang login
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            long userId = long.Parse(userIdClaim.Value);
            
            // T�m Guest tuong ?ng v?i User
            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
            if (guest == null)
            {
                // Auto-create Guest if missing
                var user = await _context.Users.FindAsync(userId);
                guest = new HotelManagementSystem.Models.Entities.Guest
                {
                    UserId = userId,
                    FullName = user?.Username ?? "Guest", // Fallback name
                    Email = user?.Email,
                    CreatedAt = DateTime.Now
                };
                _context.Guests.Add(guest);
                await _context.SaveChangesAsync();
            }

            long guestId = guest.Id;

            var room = await _roomService.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            long bookingId;

            // If ratePlanId is 0 (from Search view), auto-select default RatePlan
            if (ratePlanId == 0)
            {
                var validRatePlan = await _context.RatePlans
                    .FirstOrDefaultAsync(rp => rp.RoomTypeId == room.RoomTypeId 
                                            && checkInDate >= rp.StartDate 
                                            && checkOutDate <= rp.EndDate);
                
                if (validRatePlan != null)
                {
                    ratePlanId = validRatePlan.Id;
                }
                else
                {
                    // Fallback: Pick any rate plan for that RoomType
                    var anyRatePlan = await _context.RatePlans
                        .FirstOrDefaultAsync(rp => rp.RoomTypeId == room.RoomTypeId);
                        
                    if (anyRatePlan != null) 
                    {
                        ratePlanId = anyRatePlan.Id;
                    }
                    else 
                    {
                        // Auto-create a default RatePlan if none exists
                        var defaultRatePlan = new HotelManagementSystem.Models.Entities.RatePlan
                        {
                            RoomTypeId = room.RoomTypeId,
                            Name = "Standard Rate (Auto)",
                            Price = 1000000, 
                            StartDate = DateTime.Today.AddYears(-1),
                            EndDate = DateTime.Today.AddYears(1),
                            Type = "Flexible",
                            CreatedAt = DateTime.Now
                        };
                        _context.RatePlans.Add(defaultRatePlan);
                        await _context.SaveChangesAsync();
                        ratePlanId = defaultRatePlan.Id;
                    }
                }
            }

            try
            {
                bookingId = await _bookingService.CreateBookingAsync(
                    room.HotelId,
                    guestId,
                    room.Id,
                    ratePlanId,
                    checkInDate,
                    checkOutDate,
                    numberOfGuests
                );

                // Set initial status to Pending
                var bookingEntity = await _context.Bookings.FindAsync(bookingId);
                if (bookingEntity != null)
                {
                    bookingEntity.Status = "Pending";
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner Error: " + ex.InnerException.Message;
                }
                ModelState.AddModelError(string.Empty, errorMessage);

                var vm = new BookingSearchViewModel
                {
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    NumberOfGuests = numberOfGuests,
                    HotelId = room.HotelId,
                    RoomTypeId = room.RoomTypeId,
                    AvailableRooms = await _roomService.SearchAvailableRoomsAsync(
                        checkInDate,
                        checkOutDate,
                        numberOfGuests,
                        room.HotelId,
                        room.RoomTypeId)
                };

                return View("Search", vm);
            }

            return RedirectToAction("Payment", new { id = bookingId });
        }

        // GET: /Booking/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            try
            {
                var booking = await _bookingService.GetByIdAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                // If unpaid, redirect to Payment
                if (booking.PaymentStatus == "Unpaid" && (booking.Status == "Pending" || booking.Status == "AwaitingPayment"))
                {
                    return RedirectToAction("Payment", new { id = id });
                }

                // Nếu đã thanh toán và có invoice, redirect đến Invoice
                if (booking.PaymentStatus == "Paid" && !string.IsNullOrEmpty(booking.InvoiceNumber))
                {
                    return RedirectToAction("Invoice", new { id = id });
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Search");
            }
        }

        // GET: /Booking/Payment/5
        [HttpGet]
        public async Task<IActionResult> Payment(long id)
        {
            try
            {
                var booking = await _bookingService.GetByIdAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                // Verify ownership
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    long userId = long.Parse(userIdClaim.Value);
                    var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                    if (guest == null || booking.GuestId != guest.Id)
                    {
                        return Forbid();
                    }
                }

                // If already paid, redirect to Invoice
                if (booking.PaymentStatus == "Paid")
                {
                    return RedirectToAction("Invoice", new { id = id });
                }

                // If cancelled, redirect to MyBookings
                if (booking.Status == "Cancelled")
                {
                    TempData["ErrorMessage"] = "Booking này đã bị hủy.";
                    return RedirectToAction("MyBookings", "Customer");
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Search");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                // Verify ownership
                var booking = await _bookingService.GetByIdAsync(id);
                if (booking == null) return NotFound();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    long userId = long.Parse(userIdClaim.Value);
                    var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                    
                    if (guest == null || booking.GuestId != guest.Id)
                    {
                        return Forbid();
                    }
                }

                var result = await _bookingService.CancelBookingAsync(id, "Khách hàng hủy qua web");
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi hủy: " + ex.Message;
            }

            return RedirectToAction("MyBookings", "Customer");
        }
        [HttpGet]
        public async Task<IActionResult> Modify(long id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return NotFound();

            // Verify ownership
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                long userId = long.Parse(userIdClaim.Value);
                var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                if (guest == null || booking.GuestId != guest.Id) return Forbid();
            }

            if (booking.Status != "Confirmed" && booking.Status != "Pending" && booking.Status != "AwaitingPayment")
            {
                TempData["ErrorMessage"] = "Chưa thể chỉnh sửa booking đang chờ hoặc đã xác nhận.";
                return RedirectToAction("MyBookings", "Customer");
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(long id, DateTime newCheckInDate, DateTime newCheckOutDate)
        {
            try
            {
                // Verify ownership logic repeated... (should be refactored ideally)
                var booking = await _bookingService.GetByIdAsync(id);
                if (booking == null) return NotFound();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    long userId = long.Parse(userIdClaim.Value);
                    var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                    if (guest == null || booking.GuestId != guest.Id) return Forbid();
                }

                var result = await _bookingService.ModifyBookingAsync(id, newCheckInDate, newCheckOutDate, null);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("MyBookings", "Customer");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    // Reload view with current data but show error
                    return View(booking);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("MyBookings", "Customer");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPromotion(long bookingId, string promotionCode)
        {
            try
            {
                // Verify ownership
                var booking = await _bookingService.GetByIdAsync(bookingId);
                if (booking == null) return NotFound();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    long userId = long.Parse(userIdClaim.Value);
                    var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                    if (guest == null || booking.GuestId != guest.Id) return Forbid();
                }

                var result = await _bookingService.ApplyPromotionAsync(bookingId, promotionCode);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Payment", new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePromotion(long bookingId)
        {
            try
            {
                // Verify ownership
                var booking = await _bookingService.GetByIdAsync(bookingId);
                if (booking == null) return NotFound();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    long userId = long.Parse(userIdClaim.Value);
                    var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                    if (guest == null || booking.GuestId != guest.Id) return Forbid();
                }

                var success = await _bookingService.RemovePromotionAsync(bookingId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa mã khuyến mãi thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xoá mã khuyến mãi";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Payment", new { id = bookingId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(long bookingId, decimal amount, string paymentMethod)
        {
            try
            {
                var booking = await _bookingService.GetByIdAsync(bookingId);
                if (booking == null) return NotFound();

                // Verify ownership (simplified)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    long userId = long.Parse(userIdClaim.Value);
                    var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                    if (guest == null || booking.GuestId != guest.Id) return Forbid();
                }

                if (booking.PaymentStatus == "Paid")
                {
                    TempData["ErrorMessage"] = "Booking đã được thanh toán.";
                    return RedirectToAction("Payment", new { id = bookingId });
                }

                var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);

                if (paymentMethod == "PayAtProperty")
                {
                    // 1. Create Payment (Pending)
                    var payment = new HotelManagementSystem.Models.Entities.Payment
                    {
                        BookingId = bookingId,
                        Amount = amount,
                        Method = "Mock", // Force 'Mock' to satisfy DB ENUM constraint
                        Status = "Unpaid",
                        TxnCode = "PAY-AT-PROP-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        CreatedAt = DateTime.Now
                    };
                    _context.Payments.Add(payment);

                    // 2. Update Booking Status to Confirmed
                    var bookingEntity = await _context.Bookings.FindAsync(bookingId);
                    if (bookingEntity != null)
                    {
                        bookingEntity.Status = "Confirmed";
                    }
                    
                    // 3. Create/Update Invoice (Unpaid)
                    if (existingInvoice != null)
                    {
                        existingInvoice.Amount = amount;
                        existingInvoice.Status = "Unpaid";
                        existingInvoice.PaidAt = null;
                        existingInvoice.PaymentMethod = "PayAtProperty";
                        existingInvoice.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        var invoice = new HotelManagementSystem.Models.Entities.Invoice
                        {
                            BookingId = bookingId,
                            Number = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "-" + bookingId,
                            Amount = amount,
                            Status = "Unpaid",
                            IssuedAt = DateTime.Now,
                            PaidAt = null,
                            PaymentMethod = "PayAtProperty",
                            CreatedAt = DateTime.Now
                        };
                        _context.Invoices.Add(invoice);
                    }

                    try 
                    {
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đặt phòng thành công! Vui lòng thanh toán khi nhận phòng.";
                        return RedirectToAction("MyBookings", "Customer");
                    }
                    catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                    {
                        // Handle race condition: Clear context to avoid 'Modified' state issues
                        _context.ChangeTracker.Clear();

                        // Check if invoice actually exists now (committed by another thread)
                        var check = await _context.Invoices.AsNoTracking().AnyAsync(i => i.BookingId == bookingId);
                        if (check)
                        {
                             TempData["SuccessMessage"] = "Đặt phòng thành công (đã ghi nhận hóa đơn).";
                             return RedirectToAction("MyBookings", "Customer");
                        }
                        throw;
                    }
                }
                else
                {
                    // Online Payment (Simulated)
                    // 1. Create Payment Record
                    var payment = new HotelManagementSystem.Models.Entities.Payment
                    {
                        BookingId = bookingId,
                        Amount = amount,
                        Method = "Mock", // Force 'Mock' to satisfy DB ENUM constraint
                        Status = "Paid",
                        TxnCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        CreatedAt = DateTime.Now
                    };
                    _context.Payments.Add(payment);

                    // 2. Update Booking Status
                    var bookingEntity = await _context.Bookings.FindAsync(bookingId);
                    if (bookingEntity != null)
                    {
                        bookingEntity.PaymentStatus = "Paid";
                        bookingEntity.Status = "Confirmed";
                    }

                    // 3. Create/Update Invoice
                    if (existingInvoice != null)
                    {
                        existingInvoice.Amount = amount;
                        existingInvoice.Status = "Paid";
                        existingInvoice.PaidAt = DateTime.Now;
                        existingInvoice.PaymentMethod = paymentMethod;
                        existingInvoice.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        var invoice = new HotelManagementSystem.Models.Entities.Invoice
                        {
                            BookingId = bookingId,
                            Number = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "-" + bookingId,
                            Amount = amount,
                            Status = "Paid",
                            IssuedAt = DateTime.Now,
                            PaidAt = DateTime.Now,
                            PaymentMethod = paymentMethod,
                            CreatedAt = DateTime.Now
                        };
                        _context.Invoices.Add(invoice);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đã thanh toán thành công! Vui lòng chờ xác nhận từ khách sạn.";
                        return RedirectToAction("MyBookings", "Customer");
                    }
                    catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                    {
                        _context.ChangeTracker.Clear();
                        var check = await _context.Invoices.AsNoTracking().AnyAsync(i => i.BookingId == bookingId);
                        if (check)
                        {
                             TempData["SuccessMessage"] = "Đã thanh toán thành công (đã cập nhật hóa đơn).";
                             return RedirectToAction("MyBookings", "Customer");
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Detail: " + ex.InnerException.Message;
                }
                TempData["ErrorMessage"] = "Lỗi thanh toán: " + errorMsg;
                return RedirectToAction("Payment", new { id = bookingId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(long id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Hotel)
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room).ThenInclude(r => r.RoomType)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            // Verify ownership
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                long userId = long.Parse(userIdClaim.Value);
                var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
                if (guest == null || booking.GuestId != guest.Id) return Forbid();
            }

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == id);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Chưa có hóa đơn cho booking này.";
                return RedirectToAction("Details", new { id = id });
            }

            // Map to ViewModel
            var room = booking.BookingRooms.FirstOrDefault()?.Room;
            var roomType = booking.BookingRooms.FirstOrDefault()?.Room?.RoomType;
            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            
            var viewModel = new HotelManagementSystem.Models.ViewModels.Invoice.InvoiceDetailViewModel
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.Number,
                IssuedAt = invoice.IssuedAt,
                Status = invoice.Status,
                Notes = invoice.Notes,
                
                // Customer Info
                CustomerName = booking.Guest.FullName,
                CustomerEmail = booking.Guest.Email ?? "",
                CustomerPhone = booking.Guest.Phone ?? "",
                
                // Booking Info
                BookingId = booking.Id,
                HotelName = booking.Hotel.Name,
                HotelAddress = booking.Hotel.Address ?? "",
                RoomNumber = room?.Number ?? "N/A",
                RoomTypeName = roomType?.Name ?? "N/A",
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Nights = nights > 0 ? nights : 1,
                
                // Financials
                RoomPricePerNight = booking.BookingRooms.FirstOrDefault()?.PricePerNight ?? 0,
                RoomTotal = booking.TotalAmount + booking.DiscountAmount,
                DiscountAmount = booking.DiscountAmount,
                PromotionCode = booking.Promotion?.Code,
                TotalAmount = booking.TotalAmount,
                
                // Payment Info
                PaymentInfo = new HotelManagementSystem.Models.ViewModels.Payment.PaymentSummaryViewModel
                {
                    TotalAmount = booking.TotalAmount,
                    PaidAmount = invoice.Status == "Paid" ? booking.TotalAmount : 0, 
                    RemainingAmount = invoice.Status == "Paid" ? 0 : booking.TotalAmount,
                    // IsFullyPaid is read-only, allowed to be calculated
                    PaymentMethod = invoice.PaymentMethod ?? "N/A",
                    LastPaymentDate = invoice.PaidAt
                }
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> RoomDetails(long roomId, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
        {
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .ThenInclude(rt => rt.RoomTypeAmenities)
                .ThenInclude(rta => rta.Amenity)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound();
            }

            var viewModel = new HotelManagementSystem.Models.ViewModels.Booking.RoomDetailsViewModel
            {
                RoomId = room.Id,
                RoomNumber = room.Number,
                Floor = room.Floor,
                Status = room.Status,
                RoomTypeName = room.RoomType.Name,
                Description = room.RoomType.Description,
                ImageUrl = room.RoomType.DefaultImageUrl,
                Capacity = room.RoomType.Capacity,
                BasePrice = room.RoomType.BasePrice,
                HotelName = room.Hotel.Name,
                HotelAddress = room.Hotel.Address,
                Amenities = room.RoomType.RoomTypeAmenities.Select(rta => rta.Amenity.Name).ToList(),
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                NumberOfGuests = numberOfGuests
            };

            return View(viewModel);
        }
    }
}
