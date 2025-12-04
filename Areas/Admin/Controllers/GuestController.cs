using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Guest;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;

        public GuestController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        // GET: Admin/Guest/Index
        public async Task<IActionResult> Index(string? keyword)
        {
            var guests = string.IsNullOrWhiteSpace(keyword)
                ? await _guestService.GetAllAsync()
                : await _guestService.SearchAsync(keyword);

            ViewBag.Keyword = keyword;
            return View(guests);
        }

        // GET: Admin/Guest/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(guest);
        }

        // GET: Admin/Guest/Create
        public IActionResult Create()
        {
            return View(new GuestViewModel());
        }

        // POST: Admin/Guest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GuestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (await _guestService.IsEmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng bởi khách hàng khác.");
                    return View(model);
                }
            }

            var result = await _guestService.CreateAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = $"Đã tạo khách hàng {model.FullName} thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo khách hàng.";
            return View(model);
        }

        // GET: Admin/Guest/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(guest);
        }

        // POST: Admin/Guest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, GuestViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate email uniqueness (exclude current)
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (await _guestService.IsEmailExistsAsync(model.Email, model.Id))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng bởi khách hàng khác.");
                    return View(model);
                }
            }

            var result = await _guestService.UpdateAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin khách hàng {model.FullName} thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật khách hàng.";
            return View(model);
        }

        // GET: Admin/Guest/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(guest);
        }

        // POST: Admin/Guest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var result = await _guestService.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã xóa khách hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng. Khách hàng có thể đã có booking hoặc không tồn tại.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Search guests by keyword
        [HttpGet]
        public async Task<IActionResult> SearchGuests(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { success = false, message = "Từ khóa tìm kiếm không được để trống." });
            }

            var guests = await _guestService.SearchAsync(keyword);
            
            return Json(new 
            { 
                success = true, 
                data = guests.Select(g => new 
                {
                    id = g.Id,
                    fullName = g.FullName,
                    email = g.Email,
                    phone = g.Phone,
                    idNumber = g.IdNumber,
                    totalBookings = g.TotalBookings,
                    customerType = g.CustomerTypeDisplay
                })
            });
        }

        // API: Find guest by email
        [HttpGet]
        public async Task<IActionResult> FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { found = false, message = "Email không được để trống." });
            }

            var guest = await _guestService.FindByEmailAsync(email);
            if (guest == null)
            {
                return Json(new { found = false, message = "Không tìm thấy khách hàng với email này." });
            }

            return Json(new 
            { 
                found = true, 
                data = new 
                {
                    id = guest.Id,
                    fullName = guest.FullName,
                    email = guest.Email,
                    phone = guest.Phone,
                    idNumber = guest.IdNumber,
                    isNewCustomer = guest.IsNewCustomer
                }
            });
        }

        // API: Find guest by phone
        [HttpGet]
        public async Task<IActionResult> FindByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return Json(new { found = false, message = "Số điện thoại không được để trống." });
            }

            var guest = await _guestService.FindByPhoneAsync(phone);
            if (guest == null)
            {
                return Json(new { found = false, message = "Không tìm thấy khách hàng với số điện thoại này." });
            }

            return Json(new 
            { 
                found = true, 
                data = new 
                {
                    id = guest.Id,
                    fullName = guest.FullName,
                    email = guest.Email,
                    phone = guest.Phone,
                    idNumber = guest.IdNumber,
                    isNewCustomer = guest.IsNewCustomer
                }
            });
        }
    }
}
