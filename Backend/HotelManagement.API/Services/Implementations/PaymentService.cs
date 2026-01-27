using AutoMapper;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto dto)
    {
        // Validate
        var errors = new List<string>();
        if (dto.Amount <= 0)
            errors.Add("Payment amount must be greater than 0.");
        if (string.IsNullOrWhiteSpace(dto.Method))
            errors.Add("Payment method is required.");

        if (errors.Any())
            throw new ValidationException(errors);

        // Verify booking exists
        var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId);
        if (booking == null)
            throw new NotFoundException("Booking", dto.BookingId);

        // Check if booking is cancelled
        if (booking.Status == "Cancelled")
            throw new BusinessException("Cannot process payment for cancelled booking.");

        // Calculate total paid amount
        var totalPaid = await _unitOfWork.Payments.GetTotalPaidAsync(dto.BookingId);
        var remainingAmount = booking.TotalAmount - totalPaid;

        // Validate payment amount
        if (dto.Amount > remainingAmount)
            throw new BusinessException($"Payment amount (${dto.Amount}) exceeds remaining amount (${remainingAmount}).");

        // Create payment
        var payment = _mapper.Map<Payment>(dto);
        payment.Status = "Paid";
        payment.CreatedAt = DateTime.Now;

        await _unitOfWork.Payments.AddAsync(payment);

        // Update booking payment status
        var newTotalPaid = totalPaid + dto.Amount;
        if (newTotalPaid >= booking.TotalAmount)
        {
            booking.PaymentStatus = "Paid";
        }
        else
        {
            booking.PaymentStatus = "Partial";
        }

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByBookingAsync(long bookingId)
    {
        var payments = await _unitOfWork.Payments.GetPaymentsByBookingAsync(bookingId);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> GetPaymentByIdAsync(long id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        if (payment == null)
            throw new NotFoundException("Payment", id);

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<decimal> GetTotalPaidForBookingAsync(long bookingId)
    {
        return await _unitOfWork.Payments.GetTotalPaidAsync(bookingId);
    }

    public async Task<PaymentDto> RefundPaymentAsync(long paymentId, string reason)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
        if (payment == null)
            throw new NotFoundException("Payment", paymentId);

        if (payment.Status == "Refunded")
            throw new BusinessException("Payment has already been refunded.");

        // Create refund payment (negative amount)
        var refundPayment = new Payment
        {
            BookingId = payment.BookingId,
            Method = payment.Method,
            Amount = -payment.Amount,
            Status = "Refunded",
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Payments.AddAsync(refundPayment);

        // Update original payment status
        payment.Status = "Refunded";
        _unitOfWork.Payments.Update(payment);

        // Update booking payment status
        var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
        if (booking != null)
        {
            var totalPaid = await _unitOfWork.Payments.GetTotalPaidAsync(payment.BookingId);
            if (totalPaid <= 0)
            {
                booking.PaymentStatus = "Unpaid";
            }
            else if (totalPaid < booking.TotalAmount)
            {
                booking.PaymentStatus = "Partial";
            }
            _unitOfWork.Bookings.Update(booking);
        }

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(refundPayment);
    }
}
