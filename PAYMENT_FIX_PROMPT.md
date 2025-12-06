# PROMPT SỬA LỖI THANH TOÁN

## VẤN ĐỀ HIỆN TẠI

1. **Sau khi thanh toán online**: Khi khách hàng bấm "Xác nhận thanh toán" ở phần thanh toán online:
   - Trang thanh toán vẫn hiển thị
   - Vẫn có thông báo "vui lòng thanh toán"
   - Trong lịch sử đặt phòng vẫn thấy nút thanh toán
   - Khi bấm "Chi tiết" thì thay vì hiện hóa đơn, nó lại hiện phần thanh toán

2. **Sau khi chọn thanh toán tại khách sạn**: Không hiển thị thông báo "Đặt phòng thành công" và không hiển thị hóa đơn trong lịch sử

## YÊU CẦU SỬA

### 1. Khi thanh toán online (CreditCard/EWallet/BankTransfer):
   - Sau khi bấm "Xác nhận thanh toán":
     - Hiển thị thông báo: "Đã thanh toán thành công! Vui lòng chờ xác nhận từ khách sạn."
     - Redirect về trang lịch sử đặt phòng (MyBookings)
     - Trong lịch sử đặt phòng:
       - KHÔNG hiển thị nút "Thanh toán" nữa
       - Hiển thị trạng thái "Đã thanh toán, chờ xác nhận"
       - Khi bấm "Chi tiết" thì hiển thị HÓA ĐƠN (Invoice), KHÔNG hiển thị phần thanh toán nữa

### 2. Khi thanh toán tại khách sạn (PayAtProperty):
   - Sau khi bấm "Xác nhận đặt phòng":
     - Hiển thị thông báo: "Đặt phòng thành công! Vui lòng thanh toán khi nhận phòng."
     - Redirect về trang lịch sử đặt phòng (MyBookings)
     - Trong lịch sử đặt phòng:
       - Hiển thị nút "Hóa đơn" để xem chi tiết
       - Khi bấm "Chi tiết" thì hiển thị HÓA ĐƠN (Invoice), KHÔNG hiển thị phần thanh toán

## CÁC FILE CẦN SỬA

### 1. Controllers/BookingController.cs - Method ProcessPayment
   - **Vấn đề**: Sau khi thanh toán online, redirect về MyBookings nhưng không có thông báo phù hợp
   - **Sửa**:
     - Sau khi thanh toán online thành công: 
       - TempData["SuccessMessage"] = "Đã thanh toán thành công! Vui lòng chờ xác nhận từ khách sạn."
       - Redirect về MyBookings
     - Sau khi chọn PayAtProperty:
       - TempData["SuccessMessage"] = "Đặt phòng thành công! Vui lòng thanh toán khi nhận phòng."
       - Redirect về MyBookings

### 2. Views/Booking/Details.cshtml
   - **Vấn đề**: Vẫn hiển thị phần thanh toán sau khi đã thanh toán
   - **Sửa điều kiện hiển thị phần thanh toán**:
     - CHỈ hiển thị phần thanh toán khi:
       - Status == "Pending" HOẶC Status == "AwaitingPayment"
       - VÀ PaymentStatus == "Unpaid"
       - VÀ Status != "Cancelled"
     - Nếu đã có Invoice (InvoiceNumber không null) và PaymentStatus == "Paid":
       - Hiển thị nút "Xem hóa đơn" thay vì phần thanh toán
       - Hoặc tự động redirect đến Invoice nếu đã thanh toán

### 3. Services/Implementations/BookingService.cs - Method MapToViewModel
   - **Vấn đề**: PaymentSummary không được tính toán và gán vào BookingViewModel
   - **Sửa**: 
     - Inject IPaymentTransactionService vào BookingService
     - Trong MapToViewModel, sau khi tạo BookingViewModel:
       - Gọi GetPaymentSummaryAsync để lấy payment summary
       - Gán vào PaymentSummary của BookingViewModel
       - Cũng cần lấy InvoiceNumber và InvoiceStatus từ Invoice nếu có

### 4. Views/Customer/MyBookings.cshtml
   - **Vấn đề**: Vẫn hiển thị nút "Thanh toán" sau khi đã thanh toán
   - **Sửa điều kiện hiển thị nút "Thanh toán"**:
     - CHỈ hiển thị nút "Thanh toán" khi:
       - PaymentSummary.IsFullyPaid == false
       - VÀ Status != "Cancelled"
       - VÀ Status != "AwaitingConfirmation" (nếu đã thanh toán online)
     - Nếu PaymentStatus == "Paid" hoặc Status == "AwaitingConfirmation":
       - KHÔNG hiển thị nút "Thanh toán"
       - Hiển thị thông báo "Đã thanh toán, chờ xác nhận" nếu Status == "AwaitingConfirmation"

### 5. Controllers/BookingController.cs - Method Details
   - **Vấn đề**: Sau khi thanh toán, khi vào Details vẫn hiển thị phần thanh toán
   - **Sửa**:
     - Kiểm tra nếu PaymentStatus == "Paid" và có Invoice:
       - Redirect đến Invoice thay vì hiển thị Details với phần thanh toán
     - Hoặc trong Details view, kiểm tra và ẩn phần thanh toán nếu đã thanh toán

## CHI TIẾT SỬA TỪNG FILE

### File 1: Controllers/BookingController.cs

**Trong method ProcessPayment, sau dòng 532 (sau khi thanh toán online thành công):**
```csharp
await _context.SaveChangesAsync();
TempData["SuccessMessage"] = "Đã thanh toán thành công! Vui lòng chờ xác nhận từ khách sạn.";
return RedirectToAction("MyBookings", "Customer");
```

**Trong method ProcessPayment, sau dòng 492 (sau khi chọn PayAtProperty):**
```csharp
await _context.SaveChangesAsync();
TempData["SuccessMessage"] = "Đặt phòng thành công! Vui lòng thanh toán khi nhận phòng.";
return RedirectToAction("MyBookings", "Customer");
```

**Trong method Details, thêm logic kiểm tra:**
```csharp
[HttpGet]
public async Task<IActionResult> Details(long id)
{
    try
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        // Nếu đã thanh toán và có invoice, redirect đến Invoice
        if (booking.PaymentStatus == "Paid" && !string.IsNullOrEmpty(booking.InvoiceNumber))
        {
            return RedirectToAction("Invoice", new { id = id });
        }

        return View(booking);
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = ex.Message;
        return RedirectToAction("Search");
    }
}
```

### File 2: Services/Implementations/BookingService.cs

**Thêm dependency injection cho IPaymentTransactionService:**
```csharp
private readonly IPaymentTransactionService _paymentTransactionService;
private readonly IInvoiceService _invoiceService;

public BookingService(..., IPaymentTransactionService paymentTransactionService, IInvoiceService invoiceService)
{
    ...
    _paymentTransactionService = paymentTransactionService;
    _invoiceService = invoiceService;
}
```

**Sửa method MapToViewModel để tính PaymentSummary:**
```csharp
private async Task<BookingViewModel> MapToViewModelAsync(Booking booking)
{
    // ... existing code ...
    
    var viewModel = new BookingViewModel
    {
        // ... existing properties ...
    };

    // Calculate PaymentSummary
    var (totalAmount, paidAmount, remainingAmount) = await _paymentTransactionService.GetPaymentSummaryAsync(booking.Id);
    viewModel.PaymentSummary = new PaymentSummaryViewModel
    {
        BookingId = booking.Id,
        TotalAmount = totalAmount,
        PaidAmount = paidAmount,
        RemainingAmount = remainingAmount,
        PaymentStatus = booking.PaymentStatus
    };

    // Get Invoice info if exists
    var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.BookingId == booking.Id);
    if (invoice != null)
    {
        viewModel.InvoiceNumber = invoice.Number;
        viewModel.InvoiceStatus = invoice.Status;
    }

    return viewModel;
}
```

**Cập nhật tất cả các method sử dụng MapToViewModel để dùng MapToViewModelAsync:**
- GetByIdAsync
- GetByGuestIdAsync
- GetAllAsync
- etc.

### File 3: Views/Booking/Details.cshtml

**Sửa điều kiện hiển thị phần thanh toán (dòng 177):**
```razor
@if ((Model.Status == "AwaitingPayment" || Model.Status == "Pending") && Model.PaymentStatus == "Unpaid" && Model.Status != "Cancelled")
{
    <!-- Payment section -->
}
else if (Model.PaymentStatus == "Paid" && !string.IsNullOrEmpty(Model.InvoiceNumber))
{
    <div class="card mt-4 border-success">
        <div class="card-header bg-success text-white">
            <h5 class="mb-0"><i class="fas fa-check-circle me-2"></i>Đã Thanh Toán</h5>
        </div>
        <div class="card-body">
            <p class="mb-3">Bạn đã thanh toán thành công. Vui lòng chờ xác nhận từ khách sạn.</p>
            <a asp-action="Invoice" asp-route-id="@Model.Id" class="btn btn-primary">
                <i class="fas fa-file-invoice me-2"></i>Xem Hóa Đơn
            </a>
        </div>
    </div>
}
```

### File 4: Views/Customer/MyBookings.cshtml

**Sửa điều kiện hiển thị nút "Thanh toán" (dòng 71):**
```razor
@if (!item.PaymentSummary.IsFullyPaid && item.Status != "Cancelled" && item.PaymentStatus != "Paid" && item.Status != "AwaitingConfirmation")
{
    <a asp-controller="Booking" asp-action="Details" asp-route-id="@item.Id" class="btn btn-outline-success">
        <i class="fas fa-credit-card"></i> Thanh toán
    </a>
}

@if (item.PaymentStatus == "Paid" && item.Status == "AwaitingConfirmation")
{
    <div class="alert alert-info mt-2 mb-0">
        <small><i class="fas fa-info-circle"></i> Đã thanh toán, chờ xác nhận</small>
    </div>
}
```

## KIỂM TRA SAU KHI SỬA

1. **Thanh toán online:**
   - Bấm "Xác nhận thanh toán" → Hiển thị thông báo "Đã thanh toán thành công! Vui lòng chờ xác nhận từ khách sạn."
   - Redirect về MyBookings
   - Trong MyBookings: KHÔNG có nút "Thanh toán", có thông báo "Đã thanh toán, chờ xác nhận"
   - Bấm "Chi tiết" → Hiển thị HÓA ĐƠN, không hiển thị phần thanh toán

2. **Thanh toán tại khách sạn:**
   - Bấm "Xác nhận đặt phòng" → Hiển thị thông báo "Đặt phòng thành công! Vui lòng thanh toán khi nhận phòng."
   - Redirect về MyBookings
   - Trong MyBookings: Có nút "Hóa đơn"
   - Bấm "Chi tiết" → Hiển thị HÓA ĐƠN

