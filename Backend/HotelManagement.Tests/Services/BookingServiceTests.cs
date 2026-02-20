using Moq;
using AutoMapper;
using FluentAssertions;
using HotelManagement.API.Models.DTOs.Booking;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Implementations;
using HotelManagement.API.Exceptions;

namespace HotelManagement.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork>          _mockUoW;
    private readonly Mock<IMapper>              _mockMapper;
    private readonly Mock<IBookingRepository>   _mockBookings;
    private readonly Mock<IHotelRepository>     _mockHotels;
    private readonly Mock<IGuestRepository>     _mockGuests;
    private readonly Mock<IRoomRepository>      _mockRooms;
    private readonly Mock<IBookingRoomRepository> _mockBookingRooms;
    private readonly Mock<IPromotionRepository> _mockPromotions;
    private readonly Mock<IHousekeepingRepository> _mockHousekeeping;
    private readonly Mock<IInvoiceRepository>   _mockInvoices;
    private readonly BookingService             _sut;

    public BookingServiceTests()
    {
        _mockUoW          = new Mock<IUnitOfWork>();
        _mockMapper       = new Mock<IMapper>();
        _mockBookings     = new Mock<IBookingRepository>();
        _mockHotels       = new Mock<IHotelRepository>();
        _mockGuests       = new Mock<IGuestRepository>();
        _mockRooms        = new Mock<IRoomRepository>();
        _mockBookingRooms = new Mock<IBookingRoomRepository>();
        _mockPromotions   = new Mock<IPromotionRepository>();
        _mockHousekeeping = new Mock<IHousekeepingRepository>();
        _mockInvoices     = new Mock<IInvoiceRepository>();

        _mockUoW.Setup(u => u.Bookings).Returns(_mockBookings.Object);
        _mockUoW.Setup(u => u.Hotels).Returns(_mockHotels.Object);
        _mockUoW.Setup(u => u.Guests).Returns(_mockGuests.Object);
        _mockUoW.Setup(u => u.Rooms).Returns(_mockRooms.Object);
        _mockUoW.Setup(u => u.BookingRooms).Returns(_mockBookingRooms.Object);
        _mockUoW.Setup(u => u.Promotions).Returns(_mockPromotions.Object);
        _mockUoW.Setup(u => u.HousekeepingTasks).Returns(_mockHousekeeping.Object);
        _mockUoW.Setup(u => u.Invoices).Returns(_mockInvoices.Object);
        _mockUoW.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _sut = new BookingService(_mockUoW.Object, _mockMapper.Object);
    }

    // ─── CreateBookingAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateBookingAsync_CheckInInPast_ThrowsValidationException()
    {
        var dto = new CreateBookingDto
        {
            HotelId     = 1,
            GuestId     = 1,
            CheckInDate  = DateTime.Today.AddDays(-2),
            CheckOutDate = DateTime.Today.AddDays(3),
            RoomIds     = new List<long> { 1 }
        };

        Func<Task> act = () => _sut.CreateBookingAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("past"));
    }

    [Fact]
    public async Task CreateBookingAsync_CheckOutBeforeCheckIn_ThrowsValidationException()
    {
        var dto = new CreateBookingDto
        {
            HotelId     = 1,
            GuestId     = 1,
            CheckInDate  = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(2),
            RoomIds     = new List<long> { 1 }
        };

        Func<Task> act = () => _sut.CreateBookingAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("after check-in"));
    }

    [Fact]
    public async Task CreateBookingAsync_NoRooms_ThrowsValidationException()
    {
        var dto = new CreateBookingDto
        {
            HotelId     = 1,
            GuestId     = 1,
            CheckInDate  = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            RoomIds     = new List<long>()
        };

        Func<Task> act = () => _sut.CreateBookingAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("room"));
    }

    [Fact]
    public async Task CreateBookingAsync_HotelNotFound_ThrowsNotFoundException()
    {
        _mockHotels.Setup(r => r.GetByIdAsync<long>(99)).ReturnsAsync((Hotel?)null);

        var dto = new CreateBookingDto
        {
            HotelId     = 99,
            GuestId     = 1,
            CheckInDate  = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            RoomIds     = new List<long> { 1 }
        };

        Func<Task> act = () => _sut.CreateBookingAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Hotel*");
    }

    [Fact]
    public async Task CreateBookingAsync_GuestNotFound_ThrowsNotFoundException()
    {
        _mockHotels.Setup(r => r.GetByIdAsync<long>(1)).ReturnsAsync(new Hotel { Id = 1, Name = "Grand" });
        _mockGuests.Setup(r => r.GetByIdAsync<long>(99)).ReturnsAsync((Guest?)null);

        var dto = new CreateBookingDto
        {
            HotelId     = 1,
            GuestId     = 99,
            CheckInDate  = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            RoomIds     = new List<long> { 1 }
        };

        Func<Task> act = () => _sut.CreateBookingAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Guest*");
    }

    // ─── GetBookingByIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetBookingByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _mockBookings.Setup(r => r.GetBookingWithDetailsAsync(999)).ReturnsAsync((Booking?)null);

        Func<Task> act = () => _sut.GetBookingByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Booking*999*");
    }

    [Fact]
    public async Task GetBookingByIdAsync_Exists_ReturnsMappedDto()
    {
        var booking = new Booking { Id = 1, Status = "Confirmed", TotalAmount = 500, CreatedAt = DateTime.UtcNow };
        var dto     = new BookingDetailDto { Id = 1, Status = "Confirmed" };

        _mockBookings.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockMapper.Setup(m => m.Map<BookingDetailDto>(booking)).Returns(dto);

        var result = await _sut.GetBookingByIdAsync(1);

        result.Should().BeEquivalentTo(dto);
    }

    // ─── UpdateBookingAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateBookingAsync_NotFound_ThrowsNotFoundException()
    {
        _mockBookings.Setup(r => r.GetByIdAsync<long>(55)).ReturnsAsync((Booking?)null);

        Func<Task> act = () => _sut.UpdateBookingAsync(55, new UpdateBookingDto());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateBookingAsync_NonPendingStatus_ThrowsBusinessException()
    {
        var booking = new Booking { Id = 2, Status = "Confirmed", CreatedAt = DateTime.UtcNow };
        _mockBookings.Setup(r => r.GetByIdAsync<long>(2)).ReturnsAsync(booking);

        Func<Task> act = () => _sut.UpdateBookingAsync(2, new UpdateBookingDto { Status = "Cancelled" });

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Only pending bookings*");
    }

    // ─── CancelBookingAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CancelBookingAsync_NotFound_ThrowsNotFoundException()
    {
        _mockBookings.Setup(r => r.GetByIdAsync<long>(999)).ReturnsAsync((Booking?)null);

        Func<Task> act = () => _sut.CancelBookingAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelBookingAsync_AlreadyCheckedIn_ThrowsBusinessException()
    {
        var booking = new Booking { Id = 3, Status = "CheckedIn", CreatedAt = DateTime.UtcNow };
        _mockBookings.Setup(r => r.GetByIdAsync<long>(3)).ReturnsAsync(booking);

        Func<Task> act = () => _sut.CancelBookingAsync(3);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*pending or confirmed*");
    }

    [Fact]
    public async Task CancelBookingAsync_PendingBooking_SetsCancelledAndReleasesRooms()
    {
        var booking = new Booking { Id = 4, Status = "Pending", CreatedAt = DateTime.UtcNow };
        var bookingRooms = new List<BookingRoom>
        {
            new() { BookingId = 4, RoomId = 101 }
        };

        _mockBookings.Setup(r => r.GetByIdAsync<long>(4)).ReturnsAsync(booking);
        _mockBookingRooms
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BookingRoom, bool>>>()))
            .ReturnsAsync(bookingRooms);
        _mockRooms.Setup(r => r.UpdateRoomStatusAsync(101, "Available")).Returns(Task.CompletedTask);

        var result = await _sut.CancelBookingAsync(4);

        result.Should().BeTrue();
        booking.Status.Should().Be("Cancelled");
        _mockRooms.Verify(r => r.UpdateRoomStatusAsync(101, "Available"), Times.Once);
    }

    // ─── CheckInAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckInAsync_NotConfirmed_ThrowsBusinessException()
    {
        var booking = new Booking { Id = 5, Status = "Pending", CreatedAt = DateTime.UtcNow };
        _mockBookings.Setup(r => r.GetByIdAsync<long>(5)).ReturnsAsync(booking);

        Func<Task> act = () => _sut.CheckInAsync(5);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Only confirmed bookings*");
    }

    [Fact]
    public async Task CheckInAsync_ConfirmedBooking_SetsStatusAndUpdatesRoomToOccupied()
    {
        var booking = new Booking { Id = 6, Status = "Confirmed", CreatedAt = DateTime.UtcNow };
        var bookingRooms = new List<BookingRoom>
        {
            new() { BookingId = 6, RoomId = 201 }
        };
        var updatedBooking = new Booking { Id = 6, Status = "CheckedIn", CreatedAt = DateTime.UtcNow };

        _mockBookings.Setup(r => r.GetByIdAsync<long>(6)).ReturnsAsync(booking);
        _mockBookingRooms
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BookingRoom, bool>>>()))
            .ReturnsAsync(bookingRooms);
        _mockRooms.Setup(r => r.UpdateRoomStatusAsync(201, "Occupied")).Returns(Task.CompletedTask);
        _mockBookings.Setup(r => r.GetBookingWithDetailsAsync(6)).ReturnsAsync(updatedBooking);
        _mockMapper.Setup(m => m.Map<BookingDto>(updatedBooking)).Returns(new BookingDto { Id = 6, Status = "CheckedIn" });

        var result = await _sut.CheckInAsync(6);

        result.Status.Should().Be("CheckedIn");
        _mockRooms.Verify(r => r.UpdateRoomStatusAsync(201, "Occupied"), Times.Once);
    }

    // ─── CheckOutAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckOutAsync_NotCheckedIn_ThrowsBusinessException()
    {
        var booking = new Booking { Id = 7, Status = "Confirmed", CreatedAt = DateTime.UtcNow };
        _mockBookings.Setup(r => r.GetByIdAsync<long>(7)).ReturnsAsync(booking);

        Func<Task> act = () => _sut.CheckOutAsync(7);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Only checked-in bookings*");
    }

    [Fact]
    public async Task CheckOutAsync_CheckedInBooking_SetsMaintenanceAndCreatesHousekeepingTask()
    {
        var booking = new Booking { Id = 8, Status = "CheckedIn", TotalAmount = 500, CreatedAt = DateTime.UtcNow };
        var bookingRooms = new List<BookingRoom>
        {
            new() { BookingId = 8, RoomId = 301 }
        };
        var updatedBooking = new Booking { Id = 8, Status = "CheckedOut", TotalAmount = 500, CreatedAt = DateTime.UtcNow };

        _mockBookings.Setup(r => r.GetByIdAsync<long>(8)).ReturnsAsync(booking);
        _mockBookingRooms
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BookingRoom, bool>>>()))
            .ReturnsAsync(bookingRooms);
        _mockRooms.Setup(r => r.UpdateRoomStatusAsync(301, "Maintenance")).Returns(Task.CompletedTask);
        _mockHousekeeping.Setup(r => r.AddAsync(It.IsAny<HousekeepingTask>())).ReturnsAsync(new HousekeepingTask());
        _mockInvoices
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Invoice, bool>>>()))
            .ReturnsAsync(new List<Invoice>()); // No existing invoice
        _mockInvoices.Setup(r => r.AddAsync(It.IsAny<Invoice>())).ReturnsAsync(new Invoice());
        _mockBookings.Setup(r => r.GetBookingWithDetailsAsync(8)).ReturnsAsync(updatedBooking);
        _mockMapper.Setup(m => m.Map<BookingDto>(updatedBooking)).Returns(new BookingDto { Id = 8, Status = "CheckedOut" });

        var result = await _sut.CheckOutAsync(8);

        result.Status.Should().Be("CheckedOut");
        _mockRooms.Verify(r => r.UpdateRoomStatusAsync(301, "Maintenance"), Times.Once);
        _mockHousekeeping.Verify(r => r.AddAsync(It.Is<HousekeepingTask>(t =>
            t.RoomId == 301 && t.TaskType == "RoomCleaning")), Times.Once);
        _mockInvoices.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
    }

    // ─── GetAllBookingsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllBookingsAsync_ReturnsPaginatedResult()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, Status = "Confirmed", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Status = "Pending",   CreatedAt = DateTime.UtcNow }
        };

        _mockBookings.Setup(r => r.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync((bookings as IEnumerable<Booking>, 2));
        _mockMapper.Setup(m => m.Map<IEnumerable<BookingDto>>(bookings))
            .Returns(new List<BookingDto> { new() { Id = 1 }, new() { Id = 2 } });

        var result = await _sut.GetAllBookingsAsync(1, 10);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
    }
}
