using HotelManagementSystem.Models.ViewModels.Invoice;

namespace HotelManagementSystem.Services.Interfaces;

public interface IInvoiceService
{
    /// <summary>
    /// Lấy danh sách tất cả invoice với filter
    /// </summary>
    Task<List<InvoiceViewModel>> GetAllAsync(string? status = null, ulong? bookingId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Lấy invoice theo ID
    /// </summary>
    Task<InvoiceViewModel?> GetByIdAsync(ulong id);

    /// <summary>
    /// Lấy invoice theo Booking ID
    /// </summary>
    Task<InvoiceViewModel?> GetByBookingIdAsync(ulong bookingId);

    /// <summary>
    /// Tạo invoice mới từ booking
    /// </summary>
    Task<InvoiceViewModel?> CreateInvoiceAsync(ulong bookingId, string? notes = null);

    /// <summary>
    /// Generate số hóa đơn tự động (INV-YYYYMMDD-XXXX)
    /// </summary>
    Task<string> GenerateInvoiceNumberAsync();

    /// <summary>
    /// Cập nhật trạng thái invoice
    /// </summary>
    Task<bool> UpdateStatusAsync(ulong id, string newStatus);

    /// <summary>
    /// Đánh dấu invoice đã thanh toán
    /// </summary>
    Task<bool> MarkAsPaidAsync(ulong id, string? paymentMethod = null);

    /// <summary>
    /// Hủy invoice
    /// </summary>
    Task<bool> CancelInvoiceAsync(ulong id, string? reason = null);

    /// <summary>
    /// Generate PDF invoice (Mock - chỉ trả về đường dẫn giả lập)
    /// </summary>
    Task<string> GeneratePdfAsync(ulong id);

    /// <summary>
    /// Kiểm tra booking đã có invoice chưa
    /// </summary>
    Task<bool> HasInvoiceAsync(ulong bookingId);

    /// <summary>
    /// Lấy tổng thống kê invoice
    /// </summary>
    Task<(int TotalCount, decimal TotalAmount, int PaidCount, decimal PaidAmount)> GetStatisticsAsync();
}
