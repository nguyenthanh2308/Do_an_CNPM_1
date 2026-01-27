using AutoMapper;
using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(RoomAvailabilityDto dto)
    {
        // Validate dates
        if (dto.CheckOutDate <= dto.CheckInDate)
            throw new ValidationException("Check-out date must be after check-in date.");

        if (!dto.HotelId.HasValue)
            throw new ValidationException("Hotel ID is required.");

        // Get available rooms
        var rooms = await _unitOfWork.Rooms.GetAvailableRoomsAsync(
            dto.HotelId.Value,
            dto.CheckInDate,
            dto.CheckOutDate,
            dto.RoomTypeId);

        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }

    public async Task<RoomDto> GetRoomByIdAsync(long id)
    {
        var room = await _unitOfWork.Rooms.GetRoomWithDetailsAsync(id);
        if (room == null)
            throw new NotFoundException("Room", id);

        return _mapper.Map<RoomDto>(room);
    }

    public async Task<PaginatedResponse<RoomDto>> GetRoomsByHotelAsync(long hotelId, int pageNumber = 1, int pageSize = 10)
    {
        var rooms = await _unitOfWork.Rooms.GetRoomsByHotelAsync(hotelId);
        var roomsList = rooms.ToList();

        var pagedRooms = roomsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResponse<RoomDto>
        {
            Items = _mapper.Map<IEnumerable<RoomDto>>(pagedRooms),
            TotalCount = roomsList.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
    {
        // Validate input
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.RoomNumber))
            errors.Add("Room number is required.");
        if (dto.BasePrice <= 0)
            errors.Add("Base price must be greater than 0.");
        if (dto.Floor < 0)
            errors.Add("Floor must be non-negative.");

        if (errors.Any())
            throw new ValidationException(errors);

        // Verify hotel exists
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(dto.HotelId);
        if (hotel == null)
            throw new NotFoundException("Hotel", dto.HotelId);

        // Verify room type exists
        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(dto.RoomTypeId);
        if (roomType == null)
            throw new NotFoundException("RoomType", dto.RoomTypeId);

        // Check if room number already exists in this hotel
        var existingRooms = await _unitOfWork.Rooms.GetRoomsByHotelAsync(dto.HotelId);
        if (existingRooms.Any(r => r.RoomNumber == dto.RoomNumber))
            throw new ValidationException($"Room number {dto.RoomNumber} already exists in this hotel.");

        // Create room
        var room = _mapper.Map<Models.Entities.Room>(dto);
        room.CreatedAt = DateTime.Now;
        room.Status = "Available";

        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();

        var createdRoom = await _unitOfWork.Rooms.GetRoomWithDetailsAsync(room.Id);
        return _mapper.Map<RoomDto>(createdRoom);
    }

    public async Task<RoomDto> UpdateRoomAsync(long id, CreateRoomDto dto)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null)
            throw new NotFoundException("Room", id);

        // Update fields
        room.RoomNumber = dto.RoomNumber;
        room.RoomTypeId = dto.RoomTypeId;
        room.BasePrice = dto.BasePrice;
        room.Floor = dto.Floor;
        room.ViewType = dto.ViewType;
        room.Status = dto.Status;

        await _unitOfWork.Rooms.UpdateAsync(room);
        await _unitOfWork.SaveChangesAsync();

        var updatedRoom = await _unitOfWork.Rooms.GetRoomWithDetailsAsync(id);
        return _mapper.Map<RoomDto>(updatedRoom);
    }

    public async Task<bool> DeleteRoomAsync(long id)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null)
            throw new NotFoundException("Room", id);

        // Check if room has active bookings
        var bookingRooms = await _unitOfWork.BookingRooms.FindAsync(br => br.RoomId == id);
        if (bookingRooms.Any())
            throw new BusinessException("Cannot delete room with existing bookings.");

        _unitOfWork.Rooms.Delete(room);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateRoomStatusAsync(long id, string status)
    {
        var validStatuses = new[] { "Available", "Occupied", "Maintenance", "Cleaning", "Reserved" };
        if (!validStatuses.Contains(status))
            throw new ValidationException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

        var result = await _unitOfWork.Rooms.UpdateRoomStatusAsync(id, status);
        if (!result)
            throw new NotFoundException("Room", id);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
