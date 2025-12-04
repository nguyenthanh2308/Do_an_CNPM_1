using System.Security.Claims;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IAuthService _authService;
        private readonly HotelDbContext _context;

        public CustomerController(IBookingService bookingService, IAuthService authService, HotelDbContext context)
        {
            _bookingService = bookingService;
            _authService = authService;
            _context = context;
        }

        public async Task<IActionResult> MyBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Auth");

            long userId = long.Parse(userIdClaim.Value);
            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
            
            if (guest == null) return View(new List<HotelManagementSystem.Models.ViewModels.Booking.BookingViewModel>());

            var bookings = await _bookingService.GetByGuestIdAsync(guest.Id);
            // Filter out AwaitingPayment bookings
            bookings = bookings.Where(b => b.Status != "AwaitingPayment").ToList();
            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Auth");
            long userId = long.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);

            if (user == null) return NotFound();

            var model = new HotelManagementSystem.Models.ViewModels.Customer.ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email ?? "",
                FullName = guest?.FullName ?? "",
                Phone = guest?.Phone ?? "",
                Address = "" // Placeholder
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(HotelManagementSystem.Models.ViewModels.Customer.ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Auth");
            long userId = long.Parse(userIdClaim.Value);

            var result = await _authService.UpdateProfileAsync(userId, model.FullName, model.Email, model.Phone, model.Address ?? "");

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(HotelManagementSystem.Models.ViewModels.Customer.ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Auth");
            long userId = long.Parse(userIdClaim.Value);

            var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Profile");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
        }
    }
}
