using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Repositories.Interfaces
{
    public interface IGuestRepository : IRepository<Guest>
    {
        Task<Guest?> GetGuestByIdentityNumberAsync(string identityNumber);
        Task<IEnumerable<Guest>> SearchGuestsByNameAsync(string name);
    }

    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<Hotel?> GetHotelWithDetailsAsync(long hotelId);
    }

    public interface IRoomTypeRepository : IRepository<RoomType>
    {
        Task<IEnumerable<RoomType>> GetRoomTypesByHotelAsync(long hotelId);
        Task<RoomType?> GetRoomTypeWithAmenitiesAsync(long roomTypeId);
    }

    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByBookingAsync(long bookingId);
        Task<decimal> GetTotalPaidAsync(long bookingId);
    }

    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice?> GetInvoiceByBookingAsync(long bookingId);
    }

    public interface IHousekeepingRepository : IRepository<HousekeepingTask>
    {
        Task<IEnumerable<HousekeepingTask>> GetTasksByUserAsync(long userId);
        Task<IEnumerable<HousekeepingTask>> GetTasksByRoomAsync(long roomId);
        Task<IEnumerable<HousekeepingTask>> GetTasksByStatusAsync(string status);
        Task<IEnumerable<HousekeepingTask>> GetPendingTasksAsync();
    }

    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<Promotion?> GetPromotionByCodeAsync(string code);
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
    }

    public interface IRatePlanRepository : IRepository<RatePlan>
    {
        Task<IEnumerable<RatePlan>> GetRatePlansByRoomTypeAsync(long roomTypeId);
        Task<IEnumerable<RatePlan>> GetActiveRatePlansAsync(DateTime date);
    }

    public interface IAmenityRepository : IRepository<Amenity>
    {
        Task<IEnumerable<Amenity>> GetAmenitiesByRoomTypeAsync(long roomTypeId);
    }

    public interface IBookingRoomRepository : IRepository<BookingRoom>
    {
        Task<IEnumerable<BookingRoom>> GetByBookingIdAsync(long bookingId);
        Task<IEnumerable<BookingRoom>> GetByRoomIdAsync(long roomId);
    }

    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(long userId);
    }
}
