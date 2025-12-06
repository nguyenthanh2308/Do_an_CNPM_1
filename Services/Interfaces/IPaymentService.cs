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
        /// Kiểm tra booking đã thanh toán đầy đủ chưa
        /// </summary>
        Task<bool> IsBookingFullyPaidAsync(long bookingId);

        /// <summary>
        /// Tính tổng số tiền đã thanh toán cho booking
        /// </summary>
        Task<decimal> GetTotalPaidAmountAsync(long bookingId);
    }
}
