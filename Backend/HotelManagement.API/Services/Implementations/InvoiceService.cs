using AutoMapper;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;

namespace HotelManagement.API.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(long bookingId)
    {
        // Check if booking exists
        var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
        if (booking == null)
            throw new NotFoundException("Booking", bookingId);

        // Check if invoice already exists
        var existingInvoice = await _unitOfWork.Invoices.GetInvoiceByBookingAsync(bookingId);
        if (existingInvoice != null)
            throw new BusinessException("Invoice already exists for this booking.");

        // Create invoice with booking's total amount
        var invoice = new Invoice
        {
            BookingId = bookingId,
            InvoiceNumber = GenerateInvoiceNumber(bookingId),
            Amount = booking.TotalAmount,
            Status = "Issued",
            IssuedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        var createdInvoice = await _unitOfWork.Invoices.GetInvoiceByBookingAsync(bookingId);
        return _mapper.Map<InvoiceDto>(createdInvoice);
    }

    public async Task<InvoiceDto> GetInvoiceByBookingIdAsync(long bookingId)
    {
        var invoice = await _unitOfWork.Invoices.GetInvoiceByBookingAsync(bookingId);
        if (invoice == null)
            throw new NotFoundException($"Invoice not found for booking {bookingId}");

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> GetInvoiceByIdAsync(long id)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
        if (invoice == null)
            throw new NotFoundException("Invoice", id);

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> GetInvoiceByInvoiceNumberAsync(string invoiceNumber)
    {
        var invoice = (await _unitOfWork.Invoices.FindAsync(i => i.InvoiceNumber == invoiceNumber))
            .FirstOrDefault();

        if (invoice == null)
            throw new NotFoundException($"Invoice not found with number: {invoiceNumber}");

        return _mapper.Map<InvoiceDto>(invoice);
    }

    private string GenerateInvoiceNumber(long bookingId)
    {
        return $"INV-{bookingId:D6}-{DateTime.Now:yyyyMMddHHmmss}";
    }
}
