using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.RatePlan;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin,Manager")]
    public class RatePlanController : Controller
    {
        private readonly IRatePlanService _ratePlanService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly IHotelService _hotelService;

        public RatePlanController(
            IRatePlanService ratePlanService, 
            IRoomTypeService roomTypeService,
            IHotelService hotelService)
        {
            _ratePlanService = ratePlanService;
            _roomTypeService = roomTypeService;
            _hotelService = hotelService;
        }

        // GET: Admin/RatePlan/Index
        [HttpGet]
        public async Task<IActionResult> Index(long? roomTypeId, long? hotelId)
        {
            ViewBag.RoomTypeId = roomTypeId;
            ViewBag.HotelId = hotelId;

            // Load hotels and room types for filters
            var hotels = await _hotelService.GetAllAsync();
            ViewBag.Hotels = new SelectList(hotels, "Id", "Name", hotelId);

            if (hotelId.HasValue)
            {
                var roomTypes = await _roomTypeService.GetAllAsync(hotelId.Value);
                ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", roomTypeId);
            }

            var ratePlans = await _ratePlanService.GetAllAsync(roomTypeId, hotelId);
            return View(ratePlans);
        }

        // GET: Admin/RatePlan/Details/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Details(long id)
        {
            var ratePlan = await _ratePlanService.GetByIdAsync(id);
            if (ratePlan == null)
            {
                return NotFound();
            }

            return View(ratePlan);
        }

        // GET: Admin/RatePlan/Create
        [HttpGet]
        public async Task<IActionResult> Create(long? roomTypeId)
        {
            await LoadRoomTypesAsync();

            var model = new RatePlanViewModel
            {
                RoomTypeId = roomTypeId ?? 0,
                Type = "Flexible",
                FreeCancelUntilHours = 24
            };

            return View(model);
        }

        // POST: Admin/RatePlan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RatePlanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomTypesAsync();
                return View(model);
            }

            try
            {
                await _ratePlanService.CreateAsync(model);
                TempData["SuccessMessage"] = $"Rate plan '{model.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { roomTypeId = model.RoomTypeId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadRoomTypesAsync();
                return View(model);
            }
        }

        // GET: Admin/RatePlan/Edit/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var ratePlan = await _ratePlanService.GetByIdAsync(id);
            if (ratePlan == null)
            {
                return NotFound();
            }

            await LoadRoomTypesAsync();
            return View(ratePlan);
        }

        // POST: Admin/RatePlan/Edit/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, RatePlanViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadRoomTypesAsync();
                return View(model);
            }

            try
            {
                var updated = await _ratePlanService.UpdateAsync(model);
                if (updated == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Rate plan '{model.Name}' đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index), new { roomTypeId = model.RoomTypeId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadRoomTypesAsync();
                return View(model);
            }
        }

        // GET: Admin/RatePlan/Delete/5
        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            var ratePlan = await _ratePlanService.GetByIdAsync(id);
            if (ratePlan == null)
            {
                return NotFound();
            }

            return View(ratePlan);
        }

        // POST: Admin/RatePlan/Delete/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var ratePlan = await _ratePlanService.GetByIdAsync(id);
                var success = await _ratePlanService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Rate plan đã được xóa thành công!";
                return RedirectToAction(nameof(Index), new { roomTypeId = ratePlan?.RoomTypeId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // Helper methods
        private async Task LoadRoomTypesAsync()
        {
            var roomTypes = await _roomTypeService.GetAllAsync();
            ViewBag.RoomTypes = new SelectList(
                roomTypes.Select(rt => new 
                { 
                    rt.Id, 
                    DisplayName = $"{rt.HotelName} - {rt.Name}" 
                }), 
                "Id", 
                "DisplayName"
            );
        }
    }
}
