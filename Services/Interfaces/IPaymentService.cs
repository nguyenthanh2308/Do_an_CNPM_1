// File: Services/Interfaces/IPaymentService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Payment;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Lấy tất cả payments với filter
        /// </summary>
        Task<List<PaymentViewModel>> GetAllAsync(
            string? status = null,
            string? method = null,
            long? bookingId = null);

        /// <summary>
        /// Lấy payment theo ID
        /// </summary>
        Task<PaymentViewModel?> GetByIdAsync(long id);

        /// <summary>
        /// Lấy tất cả payments của một booking
        /// </summary>
        Task<List<PaymentViewModel>> GetByBookingIdAsync(long bookingId);

        /// <summary>
        /// Tạo payment mới
        /// </summary>
        Task<long> CreatePaymentAsync(long bookingId, string method, decimal amount);

        /// <summary>
        /// Xử lý thanh toán Mock (giả lập thành công)
        /// </summary>
        Task<(bool Success, string Message, string? TxnCode)> ProcessMockPaymentAsync(long paymentId);

        /// <summary>
        /// Xử lý thanh toán tại khách sạn
        /// </summary>
        Task<(bool Success, string Message)> ProcessPayAtPropertyAsync(long paymentId);

        /// <summary>
        /// Xử lý hoàn tiền
        /// </summary>
        Task<(bool Success, string Message)> ProcessRefundAsync(long paymentId, string reason);

        /// <summary>
        /// Đánh dấu payment thất bại
        /// </summary>
        Task<bool> MarkAsFailedAsync(long paymentId);

        /// <summary>
        /// Kiểm tra booking đã thanh toán đầy đủ chưa
        /// </summary>
        Task<bool> IsBookingFullyPaidAsync(long bookingId);

        /// <summary>
        /// Tính tổng số tiền đã thanh toán cho booking
        /// </summary>
        Task<decimal> GetTotalPaidAmountAsync(long bookingId);
    }
}
