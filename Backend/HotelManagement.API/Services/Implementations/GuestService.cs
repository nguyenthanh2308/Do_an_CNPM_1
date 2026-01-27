using AutoMapper;
using HotelManagement.API.Models.DTOs.Guest;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class GuestService : IGuestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GuestService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GuestDto> GetGuestByIdAsync(long id)
    {
        var guest = await _unitOfWork.Guests.GetByIdAsync(id);
        if (guest == null)
            throw new NotFoundException("Guest", id);

        return _mapper.Map<GuestDto>(guest);
    }

    public async Task<GuestDto?> GetGuestByEmailAsync(string email)
    {
        var guest = await _unitOfWork.Guests.GetGuestByEmailAsync(email);
        return guest != null ? _mapper.Map<GuestDto>(guest) : null;
    }

    public async Task<GuestDto?> GetGuestByIdentityNumberAsync(string idNumber)
    {
        var guest = await _unitOfWork.Guests.GetGuestByIdentityNumberAsync(idNumber);
        return guest != null ? _mapper.Map<GuestDto>(guest) : null;
    }

    public async Task<PaginatedResponse<GuestDto>> GetAllGuestsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var (guests, totalCount) = await _unitOfWork.Guests.GetPagedAsync(pageNumber, pageSize);

        return new PaginatedResponse<GuestDto>
        {
            Items = _mapper.Map<IEnumerable<GuestDto>>(guests),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<GuestDto> CreateGuestAsync(CreateGuestDto dto)
    {
        // Validate input
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.FullName))
            errors.Add("Full name is required.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required.");

        if (errors.Any())
            throw new ValidationException(errors);

        // Check if email already exists
        var existingGuest = await _unitOfWork.Guests.GetGuestByEmailAsync(dto.Email);
        if (existingGuest != null)
            throw new ValidationException($"Guest with email {dto.Email} already exists.");

        // Create guest
        var guest = _mapper.Map<Guest>(dto);
        guest.CreatedAt = DateTime.Now;

        await _unitOfWork.Guests.AddAsync(guest);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<GuestDto>(guest);
    }

    public async Task<GuestDto> UpdateGuestAsync(long id, CreateGuestDto dto)
    {
        var guest = await _unitOfWork.Guests.GetByIdAsync(id);
        if (guest == null)
            throw new NotFoundException("Guest", id);

        // Update fields
        guest.FullName = dto.FullName;
        guest.Email = dto.Email;
        guest.Phone = dto.Phone;
        guest.IdNumber = dto.IdNumber;
        guest.UserId = dto.UserId;

        await _unitOfWork.Guests.UpdateAsync(guest);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<GuestDto>(guest);
    }

    public async Task<bool> DeleteGuestAsync(long id)
    {
        var guest = await _unitOfWork.Guests.GetByIdAsync(id);
        if (guest == null)
            throw new NotFoundException("Guest", id);

        // Check if guest has bookings
        var bookings = await _unitOfWork.Bookings.GetBookingsByGuestAsync(id);
        if (bookings.Any())
            throw new BusinessException("Cannot delete guest with existing bookings.");

        _unitOfWork.Guests.Delete(guest);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
