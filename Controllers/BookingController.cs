using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Booking;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist,Customer")] // Staff và Customer có quyền booking
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
            var rooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Where(r => r.Status != "Maintenance")
                .ToListAsync();

            var viewModels = rooms.Select(r => new HotelManagementSystem.Models.ViewModels.Booking.RoomViewModel
            {
                Id = r.Id,
                Number = r.Number,
                Hotel = new HotelManagementSystem.Models.ViewModels.Hotel.HotelViewModel { Name = r.Hotel.Name },
                RoomType = new HotelManagementSystem.Models.ViewModels.RoomType.RoomTypeViewModel 
                { 
                    Name = r.RoomType.Name,
                    Description = r.RoomType.Description,
                    BasePrice = r.RoomType.BasePrice,
                    Capacity = r.RoomType.Capacity,
                    DefaultImageUrl = r.RoomType.DefaultImageUrl
                },
                Floor = (int)(r.Floor ?? 0),
                Status = r.Status,
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

            // Tìm phòng trống bằng RoomService
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
            // Lấy GuestId từ User đang login
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            long userId = long.Parse(userIdClaim.Value);
            
            // Tìm Guest tương ứng với User
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

            // Nếu ratePlanId chưa có (từ Search view), tự động lấy RatePlan mặc định
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
                    // Fallback: Lấy bất kỳ rate plan nào của RoomType đó
                    var anyRatePlan = await _context.RatePlans
                        .FirstOrDefaultAsync(rp => rp.RoomTypeId == room.RoomTypeId);
                        
                    if (anyRatePlan != null) 
                    {
                        ratePlanId = anyRatePlan.Id;
                    }
                    else 
                    {
                        // Auto-create a default RatePlan if none exists (Fix for user issue)
                        var defaultRatePlan = new HotelManagementSystem.Models.Entities.RatePlan
                        {
                            RoomTypeId = room.RoomTypeId,
                            Name = "Standard Rate (Auto)",
                            Price = 1000000, // Default price
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

                // Set initial status to AwaitingPayment so it doesn't show in history yet
                var bookingEntity = await _context.Bookings.FindAsync(bookingId);
                if (bookingEntity != null)
                {
                    bookingEntity.Status = "Pending";
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (hết phòng, date không hợp lệ, ...) → quay lại Search với message
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

            // Sau khi đặt xong có thể redirect sang trang Confirm hoặc Detail
            return RedirectToAction("Details", "Booking", new { id = bookingId });
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
                TempData["ErrorMessage"] = "Chỉ có thể chỉnh sửa booking đang chờ hoặc đã xác nhận.";
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

            return RedirectToAction("Details", new { id = bookingId });
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
                    TempData["SuccessMessage"] = "Đã xóa mã khuyến mãi";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa mã khuyến mãi";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = bookingId });
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
                    return RedirectToAction("Details", new { id = bookingId });
                }

                if (paymentMethod == "PayAtProperty")
                {
                    // 1. Create Payment (Pending)
                    var payment = new HotelManagementSystem.Models.Entities.Payment
                    {
                        BookingId = bookingId,
                        Amount = amount,
                        Method = "PayAtProperty",
                        Status = "Pending",
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
                    
                    // 3. Create Invoice (Unpaid)
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

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xác nhận thanh toán tại khách sạn! Vui lòng thanh toán khi nhận phòng.";
                }
                else
                {
                    // Online Payment (Simulated)
                    // 1. Create Payment Record
                    var payment = new HotelManagementSystem.Models.Entities.Payment
                    {
                        BookingId = bookingId,
                        Amount = amount,
                        Method = paymentMethod,
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

                    // 3. Create Invoice
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

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thanh toán thành công! Cảm ơn bạn đã sử dụng dịch vụ.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi thanh toán: " + ex.Message;
                return RedirectToAction("Details", new { id = bookingId });
            }

            // Redirect to Invoice view instead of Details (as requested by user flow)
            // But wait, user said "hoá đơn sẽ được tạo và chuyển vào phần lịch sử đặt phòng... hoá đơn chỉ có tác dụng hiển thị chi tiết"
            // And "sau đó hoá đơn sẽ được tạo và chuyển vào phần lịch sử đặt phòng của khách hàng"
            // So maybe redirect to MyBookings or stay on Details but show success?
            // The previous flow redirected to Invoice. Let's redirect to MyBookings as it seems more appropriate for "moved to history".
            // Or redirect to Details and let the user navigate.
            // User said: "chuyển vào phần lịch sử đặt phòng của khách hàng"
            
            return RedirectToAction("MyBookings", "Customer");
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(long id)
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

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == id);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Chưa có hóa đơn cho booking này.";
                return RedirectToAction("Details", new { id = id });
            }
            ViewBag.Booking = booking;
            return View(invoice);
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
