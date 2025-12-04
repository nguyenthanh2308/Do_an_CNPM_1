// File: Areas/Admin/Controllers/BookingController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IGuestService _guestService;
        private readonly IRoomService _roomService;

        public BookingController(
            IBookingService bookingService,
            IGuestService guestService,
            IRoomService roomService)
        {
            _bookingService = bookingService;
            _guestService = guestService;
            _roomService = roomService;
        }

        // GET: /Admin/Booking/Index
        [HttpGet]
        public async Task<IActionResult> Index(
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            long? guestId = null)
        {
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.GuestId = guestId;

            var bookings = await _bookingService.GetAllAsync(status, fromDate, toDate, guestId);

            // Calculate statistics
            ViewBag.TotalBookings = bookings.Count;
            ViewBag.PendingCount = bookings.Count(b => b.Status == "Pending");
            ViewBag.ConfirmedCount = bookings.Count(b => b.Status == "Confirmed");
            ViewBag.CheckedInCount = bookings.Count(b => b.Status == "CheckedIn");
            ViewBag.CompletedCount = bookings.Count(b => b.Status == "CheckedOut");
            ViewBag.CancelledCount = bookings.Count(b => b.Status == "Cancelled");
            ViewBag.TotalRevenue = bookings
                .Where(b => b.Status != "Cancelled")
                .Sum(b => b.FinalAmount);

            return View(bookings);
        }

        // GET: /Admin/Booking/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: /Admin/Booking/History?guestId=1
        [HttpGet]
        public async Task<IActionResult> History(long guestId)
        {
            var guest = await _guestService.GetByIdAsync(guestId);
            if (guest == null)
            {
                return NotFound();
            }

            var bookings = await _bookingService.GetByGuestIdAsync(guestId);

            ViewBag.GuestName = guest.FullName;
            ViewBag.GuestEmail = guest.Email;
            ViewBag.GuestPhone = guest.Phone;
            ViewBag.TotalBookings = bookings.Count;
            ViewBag.CompletedBookings = bookings.Count(b => b.Status == "CheckedOut");
            ViewBag.CancelledBookings = bookings.Count(b => b.Status == "Cancelled");
            ViewBag.TotalSpent = bookings
                .Where(b => b.Status != "Cancelled")
                .Sum(b => b.FinalAmount);

            return View(bookings);
        }

        // GET: /Admin/Booking/Cancel/5
        [HttpGet]
        public async Task<IActionResult> Cancel(long id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (!booking.IsCancellable)
            {
                TempData["ErrorMessage"] = "Không thể hủy booking này.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Calculate refund information
            var refundAmount = await _bookingService.CalculateRefundAmountAsync(id);
            ViewBag.RefundAmount = refundAmount;
            ViewBag.CanCancelFree = booking.CanCancelFree;

            return View(booking);
        }

        // POST: /Admin/Booking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(long id, string cancelReason)
        {
            var result = await _bookingService.CancelBookingAsync(id, cancelReason);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Admin/Booking/Modify/5
        [HttpGet]
        public async Task<IActionResult> Modify(long id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (!booking.IsModifiable)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa booking này.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(booking);
        }

        // POST: /Admin/Booking/ModifyDates/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyDates(
            long id,
            DateTime newCheckInDate,
            DateTime newCheckOutDate)
        {
            var result = await _bookingService.ModifyBookingAsync(
                id,
                newCheckInDate,
                newCheckOutDate,
                null);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Booking/ModifyRoom/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyRoom(long id, long newRoomId)
        {
            var result = await _bookingService.ModifyBookingAsync(
                id,
                null,
                null,
                newRoomId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Booking/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(long id)
        {
            var result = await _bookingService.CheckInAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Booking/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(long id)
        {
            var result = await _bookingService.CheckOutAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Booking/ApplyPromotion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPromotion(long id, string promotionCode)
        {
            var result = await _bookingService.ApplyPromotionAsync(id, promotionCode);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{result.Message} - Giảm giá: {result.DiscountAmount:N0} VNĐ";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Booking/RemovePromotion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePromotion(long id)
        {
            var success = await _bookingService.RemovePromotionAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa mã khuyến mãi";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa mã khuyến mãi";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // API: /Admin/Booking/ValidatePromotion
        [HttpPost]
        public async Task<IActionResult> ValidatePromotion(long bookingId, string promotionCode)
        {
            var result = await _bookingService.ApplyPromotionAsync(bookingId, promotionCode);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                discountAmount = result.DiscountAmount
            });
        }

        // API: /Admin/Booking/GetAvailableRooms
        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(
            DateTime checkInDate,
            DateTime checkOutDate,
            long hotelId,
            long? roomTypeId = null)
        {
            var rooms = await _roomService.SearchAvailableRoomsAsync(
                checkInDate,
                checkOutDate,
                1, // numberOfGuests default
                hotelId,
                roomTypeId);

            return Json(rooms.Select(r => new
            {
                id = r.Id,
                roomNumber = r.Number,
                roomTypeName = r.RoomType?.Name ?? "",
                capacity = r.RoomType?.Capacity ?? 0
            }));
        }
    }
}
