using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Booking;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist,Customer")] // Staff và Customer có quyền booking
    public class BookingController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public BookingController(IRoomService roomService, IBookingService bookingService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
        }

        // GET: /Booking/Search
        [HttpGet]
        public IActionResult Search()
        {
            // Gán default cho tiện trải nghiệm người dùng
            var model = new BookingSearchViewModel
            {
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(2),
                NumberOfGuests = 1
            };

            return View(model);
        }

        // POST: /Booking/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(BookingSearchViewModel model)
        {
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
            // Trong thực tế:
            // - GuestId lấy từ user login hoặc form nhập thông tin
            // Ở đây tạm cứng để bạn focus luồng logic, sau này đổi theo auth thực tế.
            long guestId = 1;

            var room = await _roomService.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            long bookingId;

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
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (hết phòng, date không hợp lệ, ...) → quay lại Search với message
                ModelState.AddModelError(string.Empty, ex.Message);

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
    }
}
