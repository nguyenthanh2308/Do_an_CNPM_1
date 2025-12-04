using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Hotel;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin,Manager")]
    public class HotelController : Controller
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        // GET: Admin/Hotel/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var hotels = await _hotelService.GetAllAsync();
            return View(hotels);
        }

        // GET: Admin/Hotel/Details/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Details(long id)
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // GET: Admin/Hotel/Create
        [HttpGet]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được tạo khách sạn mới
        public IActionResult Create()
        {
            var model = new HotelViewModel
            {
                Timezone = "Asia/Ho_Chi_Minh"
            };
            return View(model);
        }

        // POST: Admin/Hotel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(HotelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _hotelService.CreateAsync(model);
                TempData["SuccessMessage"] = $"Khách sạn '{model.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Admin/Hotel/Edit/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // POST: Admin/Hotel/Edit/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, HotelViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updated = await _hotelService.UpdateAsync(model);
                if (updated == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Khách sạn '{model.Name}' đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Admin/Hotel/Delete/5
        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // POST: Admin/Hotel/Delete/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var success = await _hotelService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Khách sạn đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
