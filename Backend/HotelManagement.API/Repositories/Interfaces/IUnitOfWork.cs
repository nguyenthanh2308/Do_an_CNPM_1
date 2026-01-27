namespace HotelManagement.API.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work pattern to manage repositories and database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Specific repositories
        IUserRepository Users { get; }
        IHotelRepository Hotels { get; }
        IRoomRepository Rooms { get; }
        IRoomTypeRepository RoomTypes { get; }
        IBookingRepository Bookings { get; }
        IGuestRepository Guests { get; }
        IPaymentRepository Payments { get; }
        IInvoiceRepository Invoices { get; }
        IHousekeepingRepository HousekeepingTasks { get; }
        IPromotionRepository Promotions { get; }
        IRatePlanRepository RatePlans { get; }
        IAmenityRepository Amenities { get; }
        IBookingRoomRepository BookingRooms { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IRoomTypeAmenityRepository RoomTypeAmenities { get; }

        // Transaction management
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
