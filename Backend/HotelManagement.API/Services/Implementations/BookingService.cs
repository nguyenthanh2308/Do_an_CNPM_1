using AutoMapper;
using HotelManagement.API.Models.DTOs.Booking;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
    {
        // Validate input
        var errors = new List<string>();

        if (dto.CheckInDate < DateTime.Today)
            errors.Add("Check-in date cannot be in the past.");
        if (dto.CheckOutDate <= dto.CheckInDate)
            errors.Add("Check-out date must be after check-in date.");
        if (dto.RoomIds == null || !dto.RoomIds.Any())
            errors.Add("At least one room must be selected.");

        if (errors.Any())
            throw new ValidationException(errors);

        // Verify hotel exists
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(dto.HotelId);
        if (hotel == null)
            throw new NotFoundException("Hotel", dto.HotelId);

        // Verify guest exists
        var guest = await _unitOfWork.Guests.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new NotFoundException("Guest", dto.GuestId);

        // Check room availability
        foreach (var roomId in dto.RoomIds!)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null)
                throw new NotFoundException("Room", roomId);

            if (room!.Status != "Available")
                throw new BusinessException($"Room {room.Number} is not available.");

            var availableRooms = await _unitOfWork.Rooms.GetAvailableRoomsAsync(
                dto.HotelId, dto.CheckInDate, dto.CheckOutDate);

            if (!availableRooms.Any(r => r.Id == roomId))
                throw new BusinessException($"Room {room.Number} is not available for the selected dates.");
        }

        // Calculate total amount
        decimal totalAmount = 0;
        var nights = (dto.CheckOutDate - dto.CheckInDate).Days;

        foreach (var roomId in dto.RoomIds)
        {
            var room = await _unitOfWork.Rooms.GetRoomWithDetailsAsync(roomId);
            totalAmount += (room?.RoomType?.BasePrice ?? 0) * nights;
        }

        // Apply promotion if provided
        if (dto.PromotionId.HasValue)
        {
            var promotion = await _unitOfWork.Promotions.GetByIdAsync(dto.PromotionId.Value);
            if (promotion != null && promotion.StartDate <= DateTime.Now && promotion.EndDate >= DateTime.Now)
            {
                if (promotion.Type == "Percentage")
                {
                    totalAmount -= totalAmount * (promotion.Value / 100);
                }
                else if (promotion.Type == "FixedAmount")
                {
                    totalAmount -= promotion.Value;
                }
            }
        }

        // Create booking
        var booking = _mapper.Map<Booking>(dto);
        booking.TotalAmount = totalAmount;
        booking.Status = "Pending";
        booking.PaymentStatus = "Unpaid";
        booking.CreatedAt = DateTime.Now;

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        // Create BookingRooms
        foreach (var roomId in dto.RoomIds)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null)
                throw new NotFoundException("Room", roomId);

            var bookingRoom = new BookingRoom
            {
                BookingId = booking.Id,
                RoomId = roomId,
                PricePerNight = room.RoomType?.BasePrice ?? 0,
                Nights = (dto.CheckOutDate - dto.CheckInDate).Days
            };
            await _unitOfWork.BookingRooms.AddAsync(bookingRoom);

            // Update room status
            await _unitOfWork.Rooms.UpdateRoomStatusAsync(roomId, "Reserved");
        }

        await _unitOfWork.SaveChangesAsync();

        // Return DTO
        var createdBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(booking.Id);
        return _mapper.Map<BookingDto>(createdBooking);
    }

    public async Task<BookingDetailDto> GetBookingByIdAsync(long id)
    {
        var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
        if (booking == null)
            throw new NotFoundException("Booking", id);

        return _mapper.Map<BookingDetailDto>(booking);
    }

    public async Task<PaginatedResponse<BookingDto>> GetAllBookingsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var (bookings, totalCount) = await _unitOfWork.Bookings.GetPagedAsync(pageNumber, pageSize);

        return new PaginatedResponse<BookingDto>
        {
            Items = _mapper.Map<IEnumerable<BookingDto>>(bookings),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByGuestAsync(long guestId)
    {
        var bookings = await _unitOfWork.Bookings.GetBookingsByGuestAsync(guestId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByHotelAsync(long hotelId)
    {
        var bookings = await _unitOfWork.Bookings.GetBookingsByHotelAsync(hotelId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto> UpdateBookingAsync(long id, UpdateBookingDto dto)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (booking == null)
            throw new NotFoundException("Booking", id);

        // Only allow updates for Pending bookings
        if (booking.Status != "Pending")
            throw new BusinessException("Only pending bookings can be updated.");

        // Update fields
        if (dto.CheckInDate.HasValue)
            booking.CheckInDate = dto.CheckInDate.Value;
        if (dto.CheckOutDate.HasValue)
            booking.CheckOutDate = dto.CheckOutDate.Value;
        if (!string.IsNullOrWhiteSpace(dto.Status))
            booking.Status = dto.Status;

        await _unitOfWork.Bookings.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        var updatedBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
        return _mapper.Map<BookingDto>(updatedBooking);
    }

    public async Task<BookingDto> UpdateBookingStatusAsync(long id, string status)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (booking == null)
            throw new NotFoundException("Booking", id);

        booking.Status = status;
        await _unitOfWork.Bookings.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        var updatedBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
        return _mapper.Map<BookingDto>(updatedBooking);
    }

    public async Task<bool> CancelBookingAsync(long id)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (booking == null)
            throw new NotFoundException("Booking", id);

        // Only allow cancellation for Pending or Confirmed bookings
        if (booking.Status != "Pending" && booking.Status != "Confirmed")
            throw new BusinessException("Only pending or confirmed bookings can be cancelled.");

        // Update booking status
        booking.Status = "Cancelled";
        await _unitOfWork.Bookings.UpdateAsync(booking);

        // Release rooms
        var bookingRooms = await _unitOfWork.BookingRooms.FindAsync(br => br.BookingId == id);
        foreach (var bookingRoom in bookingRooms)
        {
            if (bookingRoom.RoomId.HasValue)
                await _unitOfWork.Rooms.UpdateRoomStatusAsync(bookingRoom.RoomId.Value, "Available");
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<BookingDto> CheckInAsync(long id)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (booking == null)
            throw new NotFoundException("Booking", id);

        // Only allow check-in for Confirmed bookings
        if (booking.Status != "Confirmed")
            throw new BusinessException("Only confirmed bookings can be checked in.");

        // Update booking status
        booking.Status = "CheckedIn";
        await _unitOfWork.Bookings.UpdateAsync(booking);

        // Update room status to Occupied
        var bookingRooms = await _unitOfWork.BookingRooms.FindAsync(br => br.BookingId == id);
        foreach (var bookingRoom in bookingRooms)
        {
            if (bookingRoom.RoomId.HasValue)
                await _unitOfWork.Rooms.UpdateRoomStatusAsync(bookingRoom.RoomId.Value, "Occupied");
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
        return _mapper.Map<BookingDto>(updatedBooking);
    }

    public async Task<BookingDto> CheckOutAsync(long id)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (booking == null)
            throw new NotFoundException("Booking", id);

        // Only allow check-out for CheckedIn bookings
        if (booking.Status != "CheckedIn")
            throw new BusinessException("Only checked-in bookings can be checked out.");

        // Update booking status
        booking.Status = "CheckedOut";
        booking.CheckOutDate = DateTime.Now; // Actual checkout time
        await _unitOfWork.Bookings.UpdateAsync(booking);

        // Update room status to Maintenance and auto-create housekeeping tasks
        var bookingRooms = await _unitOfWork.BookingRooms.FindAsync(br => br.BookingId == id);
        foreach (var bookingRoom in bookingRooms)
        {
            if (bookingRoom.RoomId.HasValue)
            {
                // Update room status to Maintenance
                await _unitOfWork.Rooms.UpdateRoomStatusAsync(bookingRoom.RoomId.Value, "Maintenance");

                // Auto-create housekeeping task
                var housekeepingTask = new HousekeepingTask
                {
                    RoomId = bookingRoom.RoomId.Value,
                    TaskType = "RoomCleaning",
                    Status = "Pending",
                    Priority = "Normal",
                    AssignedToUserId = null, // Unassigned - staff will claim it
                    CreatedAt = DateTime.Now,
                    Notes = $"Auto-created after checkout of Booking #{id}"
                };
                await _unitOfWork.HousekeepingTasks.AddAsync(housekeepingTask);
            }
        }

        // Generate invoice if not exists
        var existingInvoice = (await _unitOfWork.Invoices.FindAsync(i => i.BookingId == id)).FirstOrDefault();
        if (existingInvoice == null)
        {
            var invoice = new Invoice
            {
                BookingId = id,
                Number = $"INV-{id}-{DateTime.Now:yyyyMMddHHmmss}",
                Amount = booking.TotalAmount + (booking.TotalAmount * 0.1m),
                IssuedAt = DateTime.Now,
                Status = "Issued",
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.Invoices.AddAsync(invoice);
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
        return _mapper.Map<BookingDto>(updatedBooking);
    }
}
