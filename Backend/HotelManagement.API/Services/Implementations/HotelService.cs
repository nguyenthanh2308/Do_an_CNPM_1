using AutoMapper;
using HotelManagement.API.Models.DTOs.Hotel;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class HotelService : IHotelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HotelService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
    {
        var hotels = await _unitOfWork.Hotels.GetAllAsync();
        return _mapper.Map<IEnumerable<HotelDto>>(hotels);
    }

    public async Task<HotelDto> GetHotelByIdAsync(long id)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        if (hotel == null)
            throw new NotFoundException("Hotel", id);

        return _mapper.Map<HotelDto>(hotel);
    }

    public async Task<HotelDto> CreateHotelAsync(CreateHotelDto dto)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Hotel name is required.");

        // Create hotel
        var hotel = _mapper.Map<Hotel>(dto);
        hotel.CreatedAt = DateTime.Now;

        await _unitOfWork.Hotels.AddAsync(hotel);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<HotelDto>(hotel);
    }

    public async Task<HotelDto> UpdateHotelAsync(long id, UpdateHotelDto dto)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        if (hotel == null)
            throw new NotFoundException("Hotel", id);

        // Update fields
        hotel.Name = dto.Name;
        hotel.Address = dto.Address;
        hotel.Timezone = dto.Timezone;

        _unitOfWork.Hotels.Update(hotel);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<HotelDto>(hotel);
    }

    public async Task<bool> DeleteHotelAsync(long id)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        if (hotel == null)
            throw new NotFoundException("Hotel", id);

        // Check if hotel has dependencies (rooms, bookings)
        var rooms = await _unitOfWork.Rooms.GetRoomsByHotelAsync(id);
        if (rooms.Any())
            throw new BusinessException("Cannot delete hotel with existing rooms.");

        _unitOfWork.Hotels.Delete(hotel);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<HotelDto> GetHotelWithDetailsAsync(long id)
    {
        var hotel = await _unitOfWork.Hotels.GetHotelWithDetailsAsync(id);
        if (hotel == null)
            throw new NotFoundException("Hotel", id);

        return _mapper.Map<HotelDto>(hotel);
    }
}
