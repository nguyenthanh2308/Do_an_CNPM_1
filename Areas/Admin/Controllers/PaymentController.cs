// File: Areas/Admin/Controllers/PaymentController.cs
using System;
using System.Threading.Tasks;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        // GET: /Admin/Payment/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? status = null, string? method = null)
        {
            ViewBag.Status = status;
            ViewBag.Method = method;

            var payments = await _paymentService.GetAllAsync(status, method);

            // Statistics
            ViewBag.TotalPayments = payments.Count;
            ViewBag.TotalAmount = payments.Sum(p => p.Amount);
            ViewBag.PaidCount = payments.Count(p => p.Status == "Paid");
            ViewBag.PaidAmount = payments.Where(p => p.Status == "Paid").Sum(p => p.Amount);
            ViewBag.UnpaidCount = payments.Count(p => p.Status == "Unpaid");
            ViewBag.RefundedCount = payments.Count(p => p.Status == "Refunded");
            ViewBag.RefundedAmount = payments.Where(p => p.Status == "Refunded").Sum(p => p.Amount);

            return View(payments);
        }

        // GET: /Admin/Payment/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Get booking details
            var booking = await _bookingService.GetByIdAsync(payment.BookingId);
            ViewBag.Booking = booking;

            return View(payment);
        }

        // GET: /Admin/Payment/Create?bookingId=1
        [HttpGet]
        public async Task<IActionResult> Create(long bookingId)
        {
            var booking = await _bookingService.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.IsCancelled)
            {
                TempData["ErrorMessage"] = "Không thể tạo payment cho booking đã hủy";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            ViewBag.Booking = booking;

            // Calculate remaining amount
            var totalPaid = await _paymentService.GetTotalPaidAmountAsync(bookingId);
            ViewBag.TotalPaid = totalPaid;
            ViewBag.RemainingAmount = booking.FinalAmount - totalPaid;

            return View();
        }

        // POST: /Admin/Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long bookingId, string method, decimal amount)
        {
            try
            {
                var paymentId = await _paymentService.CreatePaymentAsync(bookingId, method, amount);
                TempData["SuccessMessage"] = "Tạo payment thành công";
                return RedirectToAction(nameof(Details), new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Create), new { bookingId });
            }
        }

        // POST: /Admin/Payment/ProcessMock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMock(long id)
        {
            var result = await _paymentService.ProcessMockPaymentAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{result.Message} - Mã GD: {result.TxnCode}";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Payment/ProcessPayAtProperty/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayAtProperty(long id)
        {
            var result = await _paymentService.ProcessPayAtPropertyAsync(id);

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

        // GET: /Admin/Payment/Refund/5
        [HttpGet]
        public async Task<IActionResult> Refund(long id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            if (!payment.CanRefund)
            {
                TempData["ErrorMessage"] = "Không thể hoàn tiền cho payment này";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(payment);
        }

        // POST: /Admin/Payment/RefundConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefundConfirmed(long id, string reason)
        {
            var result = await _paymentService.ProcessRefundAsync(id, reason);

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

        // POST: /Admin/Payment/MarkFailed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFailed(long id)
        {
            var success = await _paymentService.MarkAsFailedAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã đánh dấu payment thất bại";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể đánh dấu payment này thất bại";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Admin/Payment/BookingPayments/1
        [HttpGet]
        public async Task<IActionResult> BookingPayments(long bookingId)
        {
            var booking = await _bookingService.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            var payments = await _paymentService.GetByBookingIdAsync(bookingId);

            ViewBag.Booking = booking;
            ViewBag.TotalPaid = await _paymentService.GetTotalPaidAmountAsync(bookingId);
            ViewBag.RemainingAmount = booking.FinalAmount - (decimal)ViewBag.TotalPaid;

            return View(payments);
        }
    }
}
