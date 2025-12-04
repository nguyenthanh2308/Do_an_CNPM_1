// File: Services/Interfaces/IBookingService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Booking;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Tạo booking mới:
        /// - Kiểm tra phòng còn trống (Confirmed/CheckedIn overlap).
        /// - Áp dụng giá từ RatePlan.
        /// - Lưu Booking + BookingRoom trong transaction.
        /// - Trả về BookingId.
        /// </summary>
        Task<long> CreateBookingAsync(
            long hotelId,
            long guestId,
            long roomId,
            long ratePlanId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests);

        /// <summary>
        /// Lấy tất cả bookings với filter tùy chọn
        /// </summary>
        Task<List<BookingViewModel>> GetAllAsync(
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            long? guestId = null,
            long? hotelId = null);

        /// <summary>
        /// Lấy booking theo ID
        /// </summary>
        Task<BookingViewModel?> GetByIdAsync(long id);

        /// <summary>
        /// Lấy tất cả bookings của một khách hàng
        /// </summary>
        Task<List<BookingViewModel>> GetByGuestIdAsync(long guestId);

        /// <summary>
        /// Hủy booking với logic cancellation policy
        /// </summary>
        Task<(bool Success, string Message, decimal RefundAmount)> CancelBookingAsync(
            long bookingId,
            string cancelReason);

        /// <summary>
        /// Kiểm tra có thể hủy miễn phí không
        /// </summary>
        Task<bool> CanCancelFreeAsync(long bookingId);

        /// <summary>
        /// Tính số tiền hoàn lại khi hủy
        /// </summary>
        Task<decimal> CalculateRefundAmountAsync(long bookingId);

        /// <summary>
        /// Chỉnh sửa booking (đổi ngày hoặc đổi phòng)
        /// </summary>
        Task<(bool Success, string Message)> ModifyBookingAsync(
            long bookingId,
            DateTime? newCheckInDate,
            DateTime? newCheckOutDate,
            long? newRoomId);

        /// <summary>
        /// Check-in: chuyển status từ Confirmed sang CheckedIn
        /// </summary>
        Task<(bool Success, string Message)> CheckInAsync(long bookingId);

        /// <summary>
        /// Check-out: chuyển status từ CheckedIn sang CheckedOut
        /// </summary>
        Task<(bool Success, string Message)> CheckOutAsync(long bookingId);

        /// <summary>
        /// Áp dụng promotion code vào booking
        /// </summary>
        Task<(bool Success, string Message, decimal DiscountAmount)> ApplyPromotionAsync(
            long bookingId,
            string promotionCode);

        /// <summary>
        /// Xóa promotion khỏi booking
        /// </summary>
        Task<bool> RemovePromotionAsync(long bookingId);
    }
}

