using System;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Promotion;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // GET: Admin/Promotion/Index
        public async Task<IActionResult> Index()
        {
            var promotions = await _promotionService.GetAllAsync();
            return View(promotions);
        }

        // GET: Admin/Promotion/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction(nameof(Index));
            }

            return View(promotion);
        }

        // GET: Admin/Promotion/Create
        public IActionResult Create()
        {
            var model = new PromotionViewModel
            {
                Type = "Percent",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1)
            };
            return View(model);
        }

        // POST: Admin/Promotion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Additional validation
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
                return View(model);
            }

            if (model.Type == "Percent" && (model.Value <= 0 || model.Value > 100))
            {
                ModelState.AddModelError("Value", "Giá trị phần trăm phải từ 0 đến 100.");
                return View(model);
            }

            // Check duplicate code
            if (await _promotionService.IsCodeExistsAsync(model.Code))
            {
                ModelState.AddModelError("Code", "Mã khuyến mãi đã tồn tại.");
                return View(model);
            }

            var result = await _promotionService.CreateAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = $"Đã tạo khuyến mãi {model.Code} thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo khuyến mãi.";
            return View(model);
        }

        // GET: Admin/Promotion/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction(nameof(Index));
            }

            return View(promotion);
        }

        // POST: Admin/Promotion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, PromotionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Additional validation
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
                return View(model);
            }

            if (model.Type == "Percent" && (model.Value <= 0 || model.Value > 100))
            {
                ModelState.AddModelError("Value", "Giá trị phần trăm phải từ 0 đến 100.");
                return View(model);
            }

            // Check duplicate code (exclude current)
            if (await _promotionService.IsCodeExistsAsync(model.Code, model.Id))
            {
                ModelState.AddModelError("Code", "Mã khuyến mãi đã tồn tại.");
                return View(model);
            }

            var result = await _promotionService.UpdateAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật khuyến mãi {model.Code} thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật khuyến mãi.";
            return View(model);
        }

        // GET: Admin/Promotion/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction(nameof(Index));
            }

            return View(promotion);
        }

        // POST: Admin/Promotion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var result = await _promotionService.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã xóa khuyến mãi thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa khuyến mãi. Có thể khuyến mãi đang được sử dụng.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Check if promotion code is valid
        [HttpGet]
        public async Task<IActionResult> ValidateCode(string code, decimal orderAmount = 0)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { valid = false, message = "Mã khuyến mãi không được để trống." });
            }

            var promotion = await _promotionService.GetByCodeAsync(code);
            if (promotion == null)
            {
                return Json(new { valid = false, message = "Mã khuyến mãi không tồn tại." });
            }

            if (!promotion.IsCurrentlyActive)
            {
                if (promotion.IsExpired)
                    return Json(new { valid = false, message = "Mã khuyến mãi đã hết hạn." });
                if (promotion.IsUpcoming)
                    return Json(new { valid = false, message = $"Mã khuyến mãi chưa có hiệu lực. Bắt đầu từ {promotion.StartDate:dd/MM/yyyy}." });
                
                return Json(new { valid = false, message = "Mã khuyến mãi không có hiệu lực." });
            }

            if (promotion.IsUsageLimitReached)
            {
                return Json(new { valid = false, message = "Mã khuyến mãi đã hết lượt sử dụng." });
            }

            if (promotion.MinOrderValue.HasValue && orderAmount < promotion.MinOrderValue.Value)
            {
                return Json(new 
                { 
                    valid = false, 
                    message = $"Đơn hàng tối thiểu {promotion.MinOrderValue.Value:N0} VNĐ để sử dụng mã này." 
                });
            }

            var discount = promotion.CalculateDiscount(orderAmount);
            return Json(new 
            { 
                valid = true, 
                message = $"Mã khuyến mãi hợp lệ. Giảm giá: {discount:N0} VNĐ",
                discount = discount,
                type = promotion.Type,
                value = promotion.Value
            });
        }
    }
}
