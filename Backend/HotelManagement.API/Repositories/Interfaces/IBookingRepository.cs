using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Repositories.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetBookingWithDetailsAsync(long bookingId);
        Task<IEnumerable<Booking>> GetBookingsByHotelAsync(long hotelId);
        Task<IEnumerable<Booking>> GetBookingsByGuestAsync(long guestId);
        Task<IEnumerable<Booking>> GetActiveBookingsAsync();
        Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<Booking>> GetPendingCheckInsAsync(DateTime date);
        Task<IEnumerable<Booking>> GetPendingCheckOutsAsync(DateTime date);
    }
}
