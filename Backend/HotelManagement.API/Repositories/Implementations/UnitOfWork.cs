using HotelManagement.API.Data;
using HotelManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelManagement.API.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HotelDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositories
        private IUserRepository? _users;
        private IHotelRepository? _hotels;
        private IRoomRepository? _rooms;
        private IRoomTypeRepository? _roomTypes;
        private IBookingRepository? _bookings;
        private IGuestRepository? _guests;
        private IPaymentRepository? _payments;
        private IInvoiceRepository? _invoices;
        private IHousekeepingRepository? _housekeepingTasks;
        private IPromotionRepository? _promotions;
        private IRatePlanRepository? _ratePlans;
        private IAmenityRepository? _amenities;

        public UnitOfWork(HotelDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        public IHotelRepository Hotels =>
            _hotels ??= new HotelRepository(_context);

        public IRoomRepository Rooms =>
            _rooms ??= new RoomRepository(_context);

        public IRoomTypeRepository RoomTypes =>
            _roomTypes ??= new RoomTypeRepository(_context);

        public IBookingRepository Bookings =>
            _bookings ??= new BookingRepository(_context);

        public IGuestRepository Guests =>
            _guests ??= new GuestRepository(_context);

        public IPaymentRepository Payments =>
            _payments ??= new PaymentRepository(_context);

        public IInvoiceRepository Invoices =>
            _invoices ??= new InvoiceRepository(_context);

        public IHousekeepingRepository HousekeepingTasks =>
            _housekeepingTasks ??= new HousekeepingRepository(_context);

        public IPromotionRepository Promotions =>
            _promotions ??= new PromotionRepository(_context);

        public IRatePlanRepository RatePlans =>
            _ratePlans ??= new RatePlanRepository(_context);

        public IAmenityRepository Amenities =>
            _amenities ??= new AmenityRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
