using HotelManagementSystem.Models.ViewModels.Invoice;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ManagerOrAdmin")]
public class InvoiceController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IBookingService _bookingService;

    public InvoiceController(IInvoiceService invoiceService, IBookingService bookingService)
    {
        _invoiceService = invoiceService;
        _bookingService = bookingService;
    }

    // GET: /Admin/Invoice
    public async Task<IActionResult> Index(string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var invoices = await _invoiceService.GetAllAsync(status, null, fromDate, toDate);
        var stats = await _invoiceService.GetStatisticsAsync();

        ViewBag.TotalCount = stats.TotalCount;
        ViewBag.TotalAmount = stats.TotalAmount;
        ViewBag.PaidCount = stats.PaidCount;
        ViewBag.PaidAmount = stats.PaidAmount;
        ViewBag.UnpaidCount = stats.TotalCount - stats.PaidCount;
        ViewBag.UnpaidAmount = stats.TotalAmount - stats.PaidAmount;

        ViewBag.CurrentStatus = status;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        return View(invoices);
    }

    // GET: /Admin/Invoice/Details/5
    public async Task<IActionResult> Details(ulong id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hóa đơn!";
            return RedirectToAction(nameof(Index));
        }

        return View(invoice);
    }

    // GET: /Admin/Invoice/Create?bookingId=123
    public async Task<IActionResult> Create(ulong bookingId)
    {
        // Kiểm tra booking có tồn tại không
        var booking = await _bookingService.GetByIdAsync((long)bookingId);
        if (booking == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy booking!";
            return RedirectToAction("Index", "Booking");
        }

        // Kiểm tra đã có invoice chưa
        var hasInvoice = await _invoiceService.HasInvoiceAsync(bookingId);
        if (hasInvoice)
        {
            TempData["ErrorMessage"] = "Booking này đã có hóa đơn rồi!";
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }

        ViewBag.Booking = booking!;
        return View();
    }

    // POST: /Admin/Invoice/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ulong bookingId, string? notes)
    {
        // Kiểm tra booking đã có invoice chưa
        var hasInvoice = await _invoiceService.HasInvoiceAsync(bookingId);
        if (hasInvoice)
        {
            TempData["ErrorMessage"] = "Booking này đã có hóa đơn rồi!";
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }

        var invoice = await _invoiceService.CreateInvoiceAsync(bookingId, notes);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không thể tạo hóa đơn!";
            return RedirectToAction("Create", new { bookingId });
        }

        TempData["SuccessMessage"] = $"Đã tạo hóa đơn {invoice.InvoiceNumber} thành công!";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // GET: /Admin/Invoice/BookingInvoice/123
    public async Task<IActionResult> BookingInvoice(ulong bookingId)
    {
        var invoice = await _invoiceService.GetByBookingIdAsync(bookingId);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Booking này chưa có hóa đơn!";
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }

        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // GET: /Admin/Invoice/Download/5
    public async Task<IActionResult> Download(ulong id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hóa đơn!";
            return RedirectToAction(nameof(Index));
        }

        if (!invoice.CanDownload)
        {
            TempData["ErrorMessage"] = "Chỉ có thể tải hóa đơn đã phát hành hoặc đã thanh toán!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Mock PDF generation
        var pdfPath = await _invoiceService.GeneratePdfAsync(id);

        TempData["SuccessMessage"] = $"Đã tạo file PDF: {pdfPath}";
        TempData["InfoMessage"] = "Chức năng này là mock - trong thực tế sẽ download file PDF thật.";
        
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Admin/Invoice/View/5 (Mock - preview PDF in browser)
    public async Task<IActionResult> View(ulong id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hóa đơn!";
            return RedirectToAction(nameof(Index));
        }

        if (!invoice.CanDownload)
        {
            TempData["ErrorMessage"] = "Chỉ có thể xem hóa đơn đã phát hành hoặc đã thanh toán!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Return partial view for preview
        return View("_InvoicePreview", invoice);
    }

    // POST: /Admin/Invoice/MarkAsPaid/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid(ulong id, string? paymentMethod)
    {
        var success = await _invoiceService.MarkAsPaidAsync(id, paymentMethod);
        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể đánh dấu hóa đơn đã thanh toán!";
        }
        else
        {
            TempData["SuccessMessage"] = "Đã đánh dấu hóa đơn đã thanh toán!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Invoice/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(ulong id, string? reason)
    {
        var success = await _invoiceService.CancelInvoiceAsync(id, reason);
        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể hủy hóa đơn! (Có thể hóa đơn đã thanh toán)";
        }
        else
        {
            TempData["SuccessMessage"] = "Đã hủy hóa đơn!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Invoice/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hóa đơn!";
            return RedirectToAction(nameof(Index));
        }

        // Chỉ cho phép xóa invoice Draft hoặc Cancelled
        if (invoice.Status != "Draft" && invoice.Status != "Cancelled")
        {
            TempData["ErrorMessage"] = "Chỉ có thể xóa hóa đơn ở trạng thái Nháp hoặc Đã hủy!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // In thực tế sẽ có DeleteAsync method trong service
        TempData["SuccessMessage"] = "Đã xóa hóa đơn! (Mock - chưa implement DeleteAsync)";
        return RedirectToAction(nameof(Index));
    }
}
