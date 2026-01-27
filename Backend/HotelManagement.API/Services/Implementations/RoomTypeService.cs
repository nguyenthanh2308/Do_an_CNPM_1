using AutoMapper;
using HotelManagement.API.Models.DTOs.RoomType;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class RoomTypeService : IRoomTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoomTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomTypeDto>> GetAllRoomTypesAsync()
    {
        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        return _mapper.Map<IEnumerable<RoomTypeDto>>(roomTypes);
    }

    public async Task<RoomTypeDto> GetRoomTypeByIdAsync(long id)
    {
        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id);
        if (roomType == null)
            throw new NotFoundException("RoomType", id);

        return _mapper.Map<RoomTypeDto>(roomType);
    }

    public async Task<RoomTypeDto> CreateRoomTypeAsync(CreateRoomTypeDto dto)
    {
        // Validate
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Room type name is required.");
        if (dto.Capacity <= 0)
            errors.Add("Capacity must be greater than 0.");
        if (dto.BasePrice <= 0)
            errors.Add("Base price must be greater than 0.");

        if (errors.Any())
            throw new ValidationException(errors);

        // Verify hotel exists
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(dto.HotelId);
        if (hotel == null)
            throw new NotFoundException("Hotel", dto.HotelId);

        // Create room type
        var roomType = _mapper.Map<RoomType>(dto);
        roomType.CreatedAt = DateTime.Now;

        await _unitOfWork.RoomTypes.AddAsync(roomType);
        await _unitOfWork.SaveChangesAsync();

        // Add amenities if provided
        if (dto.AmenityIds != null && dto.AmenityIds.Any())
        {
            foreach (var amenityId in dto.AmenityIds)
            {
                var amenity = await _unitOfWork.Amenities.GetByIdAsync(amenityId);
                if (amenity != null)
                {
                    var roomTypeAmenity = new RoomTypeAmenity
                    {
                        RoomTypeId = roomType.Id,
                        AmenityId = amenityId
                    };
                    await _unitOfWork.RoomTypeAmenities.AddAsync(roomTypeAmenity);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        var createdRoomType = await _unitOfWork.RoomTypes.GetRoomTypeWithAmenitiesAsync(roomType.Id);
        return _mapper.Map<RoomTypeDto>(createdRoomType);
    }

    public async Task<RoomTypeDto> UpdateRoomTypeAsync(long id, UpdateRoomTypeDto dto)
    {
        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id);
        if (roomType == null)
            throw new NotFoundException("RoomType", id);

        // Update fields
        roomType.Name = dto.Name;
        roomType.Description = dto.Description;
        roomType.Capacity = dto.Capacity;
        roomType.BasePrice = dto.BasePrice;

        _unitOfWork.RoomTypes.Update(roomType);

        // Update amenities if provided
        if (dto.AmenityIds != null)
        {
            // Remove existing amenities
            var existingAmenities = await _unitOfWork.RoomTypeAmenities.FindAsync(rta => rta.RoomTypeId == id);
            foreach (var rta in existingAmenities)
            {
                _unitOfWork.RoomTypeAmenities.Delete(rta);
            }

            // Add new amenities
            foreach (var amenityId in dto.AmenityIds)
            {
                var amenity = await _unitOfWork.Amenities.GetByIdAsync(amenityId);
                if (amenity != null)
                {
                    var roomTypeAmenity = new RoomTypeAmenity
                    {
                        RoomTypeId = id,
                        AmenityId = amenityId
                    };
                    await _unitOfWork.RoomTypeAmenities.AddAsync(roomTypeAmenity);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedRoomType = await _unitOfWork.RoomTypes.GetRoomTypeWithAmenitiesAsync(id);
        return _mapper.Map<RoomTypeDto>(updatedRoomType);
    }

    public async Task<bool> DeleteRoomTypeAsync(long id)
    {
        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id);
        if (roomType == null)
            throw new NotFoundException("RoomType", id);

        // Check if room type has rooms
        var rooms = await _unitOfWork.Rooms.FindAsync(r => r.RoomTypeId == id);
        if (rooms.Any())
            throw new BusinessException("Cannot delete room type with existing rooms.");

        _unitOfWork.RoomTypes.Delete(roomType);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<RoomTypeDto>> GetRoomTypesByHotelAsync(long hotelId)
    {
        var roomTypes = await _unitOfWork.RoomTypes.GetRoomTypesByHotelAsync(hotelId);
        return _mapper.Map<IEnumerable<RoomTypeDto>>(roomTypes);
    }

    public async Task<RoomTypeDto> GetRoomTypeWithAmenitiesAsync(long id)
    {
        var roomType = await _unitOfWork.RoomTypes.GetRoomTypeWithAmenitiesAsync(id);
        if (roomType == null)
            throw new NotFoundException("RoomType", id);

        return _mapper.Map<RoomTypeDto>(roomType);
    }
}
