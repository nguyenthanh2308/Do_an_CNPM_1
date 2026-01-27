using HotelManagement.API.Models.DTOs.Booking;
using HotelManagement.API.Models.DTOs.Common;

namespace HotelManagement.API.Services.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
    Task<BookingDetailDto> GetBookingByIdAsync(long id);
    Task<PaginatedResponse<BookingDto>> GetAllBookingsAsync(int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<BookingDto>> GetBookingsByGuestAsync(long guestId);
    Task<IEnumerable<BookingDto>> GetBookingsByHotelAsync(long hotelId);
    Task<BookingDto> UpdateBookingAsync(long id, UpdateBookingDto dto);
    Task<BookingDto> UpdateBookingStatusAsync(long id, string status);
    Task<bool> CancelBookingAsync(long id);
    Task<BookingDto> CheckInAsync(long id);
    Task<BookingDto> CheckOutAsync(long id);
}
