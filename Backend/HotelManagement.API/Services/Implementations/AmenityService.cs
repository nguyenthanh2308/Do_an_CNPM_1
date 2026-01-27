using AutoMapper;
using HotelManagement.API.Models.DTOs.Amenity;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class AmenityService : IAmenityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AmenityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync()
    {
        var amenities = await _unitOfWork.Amenities.GetAllAsync();
        return _mapper.Map<IEnumerable<AmenityDto>>(amenities);
    }

    public async Task<AmenityDto> GetAmenityByIdAsync(long id)
    {
        var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
        if (amenity == null)
            throw new NotFoundException("Amenity", id);

        return _mapper.Map<AmenityDto>(amenity);
    }

    public async Task<AmenityDto> CreateAmenityAsync(CreateAmenityDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Amenity name is required.");

        var amenity = _mapper.Map<Amenity>(dto);
        await _unitOfWork.Amenities.AddAsync(amenity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AmenityDto>(amenity);
    }

    public async Task<AmenityDto> UpdateAmenityAsync(long id, CreateAmenityDto dto)
    {
        var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
        if (amenity == null)
            throw new NotFoundException("Amenity", id);

        amenity.Name = dto.Name;

        _unitOfWork.Amenities.Update(amenity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AmenityDto>(amenity);
    }

    public async Task<bool> DeleteAmenityAsync(long id)
    {
        var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
        if (amenity == null)
            throw new NotFoundException("Amenity", id);

        _unitOfWork.Amenities.Delete(amenity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<AmenityDto>> GetAmenitiesByRoomTypeAsync(long roomTypeId)
    {
        var amenities = await _unitOfWork.Amenities.GetAmenitiesByRoomTypeAsync(roomTypeId);
        return _mapper.Map<IEnumerable<AmenityDto>>(amenities);
    }
}
