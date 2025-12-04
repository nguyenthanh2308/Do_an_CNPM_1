using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Amenity;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin,Manager")]
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;

        public AmenityController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        // GET: Admin/Amenity/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var amenities = await _amenityService.GetAllAsync();
            return View(amenities);
        }

        // GET: Admin/Amenity/Details/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Details(long id)
        {
            var amenity = await _amenityService.GetByIdAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            return View(amenity);
        }

        // GET: Admin/Amenity/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AmenityViewModel();
            return View(model);
        }

        // POST: Admin/Amenity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AmenityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _amenityService.CreateAsync(model);
                TempData["SuccessMessage"] = $"Tiện nghi '{model.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Admin/Amenity/Edit/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var amenity = await _amenityService.GetByIdAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            return View(amenity);
        }

        // POST: Admin/Amenity/Edit/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AmenityViewModel model)
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
                var updated = await _amenityService.UpdateAsync(model);
                if (updated == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Tiện nghi '{model.Name}' đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Admin/Amenity/Delete/5
        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            var amenity = await _amenityService.GetByIdAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

            return View(amenity);
        }

        // POST: Admin/Amenity/Delete/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var success = await _amenityService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Tiện nghi đã được xóa thành công!";
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
