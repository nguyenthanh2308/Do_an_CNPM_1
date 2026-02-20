using Moq;
using AutoMapper;
using FluentAssertions;
using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Implementations;
using HotelManagement.API.Exceptions;

namespace HotelManagement.Tests.Services;

public class RoomServiceTests
{
    private readonly Mock<IUnitOfWork>            _mockUoW;
    private readonly Mock<IMapper>                _mockMapper;
    private readonly Mock<IRoomRepository>        _mockRooms;
    private readonly Mock<IHotelRepository>       _mockHotels;
    private readonly Mock<IRoomTypeRepository>    _mockRoomTypes;
    private readonly Mock<IBookingRoomRepository> _mockBookingRooms;
    private readonly RoomService                  _sut;

    public RoomServiceTests()
    {
        _mockUoW          = new Mock<IUnitOfWork>();
        _mockMapper       = new Mock<IMapper>();
        _mockRooms        = new Mock<IRoomRepository>();
        _mockHotels       = new Mock<IHotelRepository>();
        _mockRoomTypes    = new Mock<IRoomTypeRepository>();
        _mockBookingRooms = new Mock<IBookingRoomRepository>();

        _mockUoW.Setup(u => u.Rooms).Returns(_mockRooms.Object);
        _mockUoW.Setup(u => u.Hotels).Returns(_mockHotels.Object);
        _mockUoW.Setup(u => u.RoomTypes).Returns(_mockRoomTypes.Object);
        _mockUoW.Setup(u => u.BookingRooms).Returns(_mockBookingRooms.Object);
        _mockUoW.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _sut = new RoomService(_mockUoW.Object, _mockMapper.Object);
    }

    // ─── GetAvailableRoomsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableRoomsAsync_CheckOutBeforeCheckIn_ThrowsValidationException()
    {
        var dto = new RoomAvailabilityDto
        {
            HotelId     = 1,
            CheckInDate  = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(2)
        };

        Func<Task> act = () => _sut.GetAvailableRoomsAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*after check-in*");
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_NoHotelId_ThrowsValidationException()
    {
        var dto = new RoomAvailabilityDto
        {
            HotelId     = null,
            CheckInDate  = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3)
        };

        Func<Task> act = () => _sut.GetAvailableRoomsAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Hotel ID*");
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_ValidInput_ReturnsMappedRooms()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Status = "Available" },
            new() { Id = 2, Number = "102", Status = "Available" }
        };

        var dto = new RoomAvailabilityDto
        {
            HotelId     = 1,
            CheckInDate  = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3)
        };

        _mockRooms
            .Setup(r => r.GetAvailableRoomsAsync(1, dto.CheckInDate, dto.CheckOutDate, null))
            .ReturnsAsync(rooms);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<RoomDto>>(rooms))
            .Returns(new List<RoomDto> { new() { Id = 1 }, new() { Id = 2 } });

        var result = await _sut.GetAvailableRoomsAsync(dto);

        result.Should().HaveCount(2);
    }

    // ─── GetRoomByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetRoomByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _mockRooms.Setup(r => r.GetRoomWithDetailsAsync(99)).ReturnsAsync((Room?)null);

        Func<Task> act = () => _sut.GetRoomByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Room*99*");
    }

    [Fact]
    public async Task GetRoomByIdAsync_Exists_ReturnsMappedDto()
    {
        var room = new Room { Id = 10, Number = "101", Status = "Available" };
        var dto  = new RoomDto { Id = 10, RoomNumber = "101" };

        _mockRooms.Setup(r => r.GetRoomWithDetailsAsync(10)).ReturnsAsync(room);
        _mockMapper.Setup(m => m.Map<RoomDto>(room)).Returns(dto);

        var result = await _sut.GetRoomByIdAsync(10);

        result.Id.Should().Be(10);
    }

    // ─── CreateRoomAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRoomAsync_EmptyRoomNumber_ThrowsValidationException()
    {
        var dto = new CreateRoomDto { RoomNumber = "", HotelId = 1, RoomTypeId = 1, BasePrice = 100, Floor = 1 };

        Func<Task> act = () => _sut.CreateRoomAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("Room number is required"));
    }

    [Fact]
    public async Task CreateRoomAsync_ZeroBasePrice_ThrowsValidationException()
    {
        var dto = new CreateRoomDto { RoomNumber = "101", HotelId = 1, RoomTypeId = 1, BasePrice = 0, Floor = 1 };

        Func<Task> act = () => _sut.CreateRoomAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("greater than 0"));
    }

    [Fact]
    public async Task CreateRoomAsync_HotelNotFound_ThrowsNotFoundException()
    {
        _mockHotels.Setup(r => r.GetByIdAsync<long>(99)).ReturnsAsync((Hotel?)null);

        var dto = new CreateRoomDto { RoomNumber = "101", HotelId = 99, RoomTypeId = 1, BasePrice = 100, Floor = 1 };

        Func<Task> act = () => _sut.CreateRoomAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Hotel*");
    }

    [Fact]
    public async Task CreateRoomAsync_RoomTypeNotFound_ThrowsNotFoundException()
    {
        _mockHotels.Setup(r => r.GetByIdAsync<long>(1)).ReturnsAsync(new Hotel { Id = 1, Name = "Grand" });
        _mockRoomTypes.Setup(r => r.GetByIdAsync<long>(99)).ReturnsAsync((RoomType?)null);

        var dto = new CreateRoomDto { RoomNumber = "101", HotelId = 1, RoomTypeId = 99, BasePrice = 100, Floor = 1 };

        Func<Task> act = () => _sut.CreateRoomAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*RoomType*");
    }

    [Fact]
    public async Task CreateRoomAsync_DuplicateRoomNumber_ThrowsValidationException()
    {
        var hotel    = new Hotel { Id = 1, Name = "Grand" };
        var roomType = new RoomType { Id = 1, Name = "Deluxe", BasePrice = 100 };
        var existing = new List<Room> { new() { Id = 5, Number = "101", HotelId = 1 } };

        _mockHotels.Setup(r => r.GetByIdAsync<long>(1)).ReturnsAsync(hotel);
        _mockRoomTypes.Setup(r => r.GetByIdAsync<long>(1)).ReturnsAsync(roomType);
        _mockRooms.Setup(r => r.GetRoomsByHotelAsync(1)).ReturnsAsync(existing);

        var dto = new CreateRoomDto { RoomNumber = "101", HotelId = 1, RoomTypeId = 1, BasePrice = 100, Floor = 1 };

        Func<Task> act = () => _sut.CreateRoomAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("101") && e.Contains("already exists"));
    }

    // ─── DeleteRoomAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteRoomAsync_NotFound_ThrowsNotFoundException()
    {
        _mockRooms.Setup(r => r.GetByIdAsync<long>(999)).ReturnsAsync((Room?)null);

        Func<Task> act = () => _sut.DeleteRoomAsync(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Room*999*");
    }

    [Fact]
    public async Task DeleteRoomAsync_HasBookings_ThrowsBusinessException()
    {
        var room = new Room { Id = 50, Number = "501", Status = "Available" };
        var bookingRooms = new List<BookingRoom>
        {
            new() { RoomId = 50, BookingId = 1 }
        };

        _mockRooms.Setup(r => r.GetByIdAsync<long>(50)).ReturnsAsync(room);
        _mockBookingRooms
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BookingRoom, bool>>>()))
            .ReturnsAsync(bookingRooms);

        Func<Task> act = () => _sut.DeleteRoomAsync(50);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*existing bookings*");
    }

    [Fact]
    public async Task DeleteRoomAsync_NoBookings_DeletesRoomAndReturnsTrue()
    {
        var room = new Room { Id = 60, Number = "601", Status = "Available" };

        _mockRooms.Setup(r => r.GetByIdAsync<long>(60)).ReturnsAsync(room);
        _mockBookingRooms
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BookingRoom, bool>>>()))
            .ReturnsAsync(new List<BookingRoom>());

        var result = await _sut.DeleteRoomAsync(60);

        result.Should().BeTrue();
        _mockRooms.Verify(r => r.Delete(room), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ─── UpdateRoomStatusAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRoomStatusAsync_InvalidStatus_ThrowsValidationException()
    {
        Func<Task> act = () => _sut.UpdateRoomStatusAsync(1, "Dirty");

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid status*");
    }

    [Fact]
    public async Task UpdateRoomStatusAsync_ValidStatus_CallsRepositoryAndSaves()
    {
        var room = new Room { Id = 70, Number = "701", Status = "Available" };

        _mockRooms.Setup(r => r.GetByIdAsync<long>(70)).ReturnsAsync(room);
        _mockRooms.Setup(r => r.UpdateRoomStatusAsync(70, "Maintenance")).Returns(Task.CompletedTask);

        var result = await _sut.UpdateRoomStatusAsync(70, "Maintenance");

        result.Should().BeTrue();
        _mockRooms.Verify(r => r.UpdateRoomStatusAsync(70, "Maintenance"), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("Available")]
    [InlineData("Occupied")]
    [InlineData("Maintenance")]
    [InlineData("Cleaning")]
    [InlineData("Reserved")]
    public async Task UpdateRoomStatusAsync_AllValidStatuses_DoNotThrow(string status)
    {
        var room = new Room { Id = 80, Number = "801", Status = "Available" };

        _mockRooms.Setup(r => r.GetByIdAsync<long>(80)).ReturnsAsync(room);
        _mockRooms.Setup(r => r.UpdateRoomStatusAsync(80, status)).Returns(Task.CompletedTask);

        Func<Task> act = () => _sut.UpdateRoomStatusAsync(80, status);

        await act.Should().NotThrowAsync();
    }

    // ─── GetRoomsByHotelAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetRoomsByHotelAsync_ReturnsCorrectPagination()
    {
        var rooms = Enumerable.Range(1, 25)
            .Select(i => new Room { Id = i, Number = $"R{i:000}", Status = "Available" })
            .ToList();

        _mockRooms.Setup(r => r.GetRoomsByHotelAsync(1)).ReturnsAsync(rooms);
        _mockMapper.Setup(m => m.Map<IEnumerable<RoomDto>>(It.IsAny<IEnumerable<Room>>()))
            .Returns<IEnumerable<Room>>(r => r.Select(x => new RoomDto { Id = x.Id }));

        var result = await _sut.GetRoomsByHotelAsync(1, pageNumber: 2, pageSize: 10);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(10);
        result.PageNumber.Should().Be(2);
    }
}
