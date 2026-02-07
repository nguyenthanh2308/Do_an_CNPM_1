# Phân Chia Controllers Theo Vai Trò

## Tổng Quan
Hệ thống Hotel Management có **14 Controllers** được chia theo 5 vai trò chính:
- **Admin** - Quản trị viên hệ thống
- **Manager** - Quản lý khách sạn
- **Receptionist** - Lễ tân
- **Housekeeping** - Nhân viên dọn phòng
- **Customer** - Khách hàng

---

## 1. PUBLIC CONTROLLERS (Không cần đăng nhập)

### ✅ AuthController
**Chức năng:** Xác thực và quản lý session
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký tài khoản mới
- `POST /api/auth/refresh-token` - Làm mới token
- `GET /api/auth/me` - Lấy thông tin user hiện tại
- `POST /api/auth/logout` - Đăng xuất

### ✅ HotelsController  
**Vai trò public:** Xem danh sách khách sạn
- `GET /api/hotels` - Danh sách khách sạn (Public)
- `GET /api/hotels/{id}` - Chi tiết khách sạn (Public)

**Vai trò Manager/Admin:** Quản lý khách sạn
- `POST /api/hotels` - Tạo khách sạn mới
- `PUT /api/hotels/{id}` - Cập nhật khách sạn
- `DELETE /api/hotels/{id}` - Xóa khách sạn

### ✅ RoomTypesController
**Vai trò public:** Xem loại phòng
- `GET /api/roomtypes` - Danh sách loại phòng
- `GET /api/roomtypes/{id}` - Chi tiết loại phòng
- `GET /api/roomtypes/hotel/{hotelId}` - Loại phòng theo khách sạn

**Vai trò Manager/Admin:** Quản lý loại phòng
- `POST /api/roomtypes` - Tạo loại phòng
- `PUT /api/roomtypes/{id}` - Cập nhật loại phòng
- `DELETE /api/roomtypes/{id}` - Xóa loại phòng

### ✅ RoomsController
**Vai trò public:** Xem phòng trống
- `GET /api/rooms/available` - Tìm phòng trống
- `GET /api/rooms/hotel/{hotelId}` - Phòng theo khách sạn

**Vai trò Receptionist+:** Quản lý trạng thái phòng
- `PUT /api/rooms/{id}/status` - Cập nhật trạng thái phòng

**Vai trò Manager/Admin:** CRUD phòng
- `POST /api/rooms` - Tạo phòng mới
- `PUT /api/rooms/{id}` - Cập nhật phòng
- `DELETE /api/rooms/{id}` - Xóa phòng

### ✅ AmenitiesController
**Vai trò public:** Xem tiện nghi
- `GET /api/amenities` - Danh sách tiện nghi
- `GET /api/amenities/{id}` - Chi tiết tiện nghi
- `GET /api/amenities/roomtype/{roomTypeId}` - Tiện nghi theo loại phòng

**Vai trò Manager/Admin:** Quản lý tiện nghi
- `POST /api/amenities` - Tạo tiện nghi
- `PUT /api/amenities/{id}` - Cập nhật tiện nghi
- `DELETE /api/amenities/{id}` - Xóa tiện nghi

### ✅ PromotionsController ⭐ MỚI
**Vai trò public:** Xem khuyến mãi
- `GET /api/promotions` - Tất cả khuyến mãi
- `GET /api/promotions/active` - Khuyến mãi đang hoạt động
- `GET /api/promotions/{id}` - Chi tiết khuyến mãi
- `GET /api/promotions/code/{code}` - Tìm theo mã code

**Vai trò đăng nhập:** Validate và tính discount
- `POST /api/promotions/validate` - Kiểm tra mã khuyến mãi
- `POST /api/promotions/calculate-discount` - Tính giảm giá

**Vai trò Manager/Admin:** CRUD khuyến mãi
- `POST /api/promotions` - Tạo khuyến mãi
- `PUT /api/promotions/{id}` - Cập nhật khuyến mãi
- `DELETE /api/promotions/{id}` - Xóa khuyến mãi

### ✅ RatePlansController ⭐ MỚI
**Vai trò public:** Xem gói giá
- `GET /api/rateplans` - Tất cả gói giá
- `GET /api/rateplans/{id}` - Chi tiết gói giá
- `GET /api/rateplans/room-type/{roomTypeId}` - Gói giá theo loại phòng
- `GET /api/rateplans/active?date=` - Gói giá active
- `GET /api/rateplans/best-rate` - Tìm gói giá tốt nhất

**Vai trò đăng nhập:** Tính giá
- `POST /api/rateplans/calculate-rate` - Tính tổng giá đặt phòng

**Vai trò Manager/Admin:** CRUD gói giá
- `POST /api/rateplans` - Tạo gói giá
- `PUT /api/rateplans/{id}` - Cập nhật gói giá
- `DELETE /api/rateplans/{id}` - Xóa gói giá

---

## 2. CUSTOMER CONTROLLERS

### ✅ BookingsController
**Customer:**
- `GET /api/bookings/my-bookings` - Xem booking của mình
- `GET /api/bookings/{id}` - Xem chi tiết booking (chỉ của mình)
- `POST /api/bookings` - Tạo booking mới
- `POST /api/bookings/{id}/cancel` - Hủy booking

**Receptionist/Manager:**
- `GET /api/bookings` - Xem tất cả bookings
- `PUT /api/bookings/{id}` - Cập nhật booking
- `POST /api/bookings/{id}/checkin` - Check-in
- `POST /api/bookings/{id}/checkout` - Check-out

### ✅ InvoicesController
**Customer:**
- `GET /api/invoices/my-invoices` - Xem hóa đơn của mình

**Receptionist/Manager:**
- `POST /api/invoices/generate` - Tạo hóa đơn
- `GET /api/invoices/booking/{bookingId}` - Hóa đơn theo booking
- `GET /api/invoices/{id}` - Chi tiết hóa đơn
- `GET /api/invoices/number/{invoiceNumber}` - Tìm theo số hóa đơn
- `GET /api/invoices/{id}/download` - Tải hóa đơn

---

## 3. RECEPTIONIST CONTROLLERS

### ✅ GuestsController
**Receptionist/Manager:**
- `GET /api/guests` - Danh sách khách (phân trang)
- `GET /api/guests/{id}` - Chi tiết khách
- `GET /api/guests/search` - Tìm khách theo CMND/Email
- `POST /api/guests` - Tạo hồ sơ khách mới
- `PUT /api/guests/{id}` - Cập nhật thông tin khách

**Manager only:**
- `DELETE /api/guests/{id}` - Xóa khách

### ✅ PaymentsController
**Receptionist/Manager:**
- `POST /api/payments/process` - Xử lý thanh toán
- `GET /api/payments/booking/{bookingId}` - Thanh toán theo booking
- `GET /api/payments/{id}` - Chi tiết thanh toán
- `GET /api/payments/booking/{bookingId}/total` - Tổng đã thanh toán

**Manager only:**
- `POST /api/payments/{id}/refund` - Hoàn tiền

---

## 4. HOUSEKEEPING CONTROLLERS

### ✅ HousekeepingController
**Housekeeping staff:**
- `GET /api/housekeeping/my-tasks` - Nhiệm vụ được giao
- `POST /api/housekeeping/{taskId}/claim` - Nhận nhiệm vụ (tự gán)
- `PUT /api/housekeeping/{taskId}/status` - Cập nhật trạng thái
- `POST /api/housekeeping/{taskId}/notes` - Thêm ghi chú
- `POST /api/housekeeping/{taskId}/report-issues` - Báo cáo vấn đề

**Receptionist/Manager:**
- `GET /api/housekeeping/pending` - Nhiệm vụ chưa gán
- `POST /api/housekeeping` - Tạo nhiệm vụ mới
- `POST /api/housekeeping/{taskId}/assign/{userId}` - Gán nhiệm vụ

**Manager only:**
- `GET /api/housekeeping` - Tất cả nhiệm vụ
- `GET /api/housekeeping/{id}` - Chi tiết nhiệm vụ
- `DELETE /api/housekeeping/{id}` - Xóa nhiệm vụ

---

## 5. MANAGER/ADMIN CONTROLLERS

### ✅ UsersController
**Manager/Admin:**
- `GET /api/users` - Danh sách users
- `GET /api/users/{id}` - Chi tiết user
- `POST /api/users` - Tạo user (Manager chỉ tạo Receptionist/Housekeeping)
- `PUT /api/users/{id}` - Cập nhật user
- `PUT /api/users/{id}/deactivate` - Vô hiệu hóa user

**Admin only:**
- `DELETE /api/users/{id}` - Xóa user

### ✅ ReportsController
**Manager/Admin:**
- `GET /api/reports/revenue` - Báo cáo doanh thu
- `GET /api/reports/occupancy` - Báo cáo tỷ lệ lấp phòng
- `GET /api/reports/popular-rooms` - Phòng phổ biến
- `GET /api/reports/guest-statistics` - Thống kê khách
- `GET /api/reports/payment-summary` - Tổng hợp thanh toán
- `GET /api/reports/housekeeping-performance` - Hiệu suất housekeeping
- `GET /api/reports/export` - Xuất báo cáo PDF

---

## MAPPING VAI TRÒ - CONTROLLERS

### 👤 CUSTOMER
- ✅ AuthController (Public)
- ✅ Hotels, RoomTypes, Rooms, Amenities (Xem)
- ✅ Promotions, RatePlans (Xem + Validate)
- ✅ BookingsController (My bookings, Create, Cancel)
- ✅ InvoicesController (My invoices)

### 🧹 HOUSEKEEPING
- ✅ AuthController
- ✅ HousekeepingController (My tasks, Claim, Update status, Report)

### 🏨 RECEPTIONIST  
- ✅ AuthController
- ✅ Hotels, RoomTypes, Rooms (Xem + Update room status)
- ✅ GuestsController (CRUD guests)
- ✅ BookingsController (All bookings, Check-in, Check-out)
- ✅ PaymentsController (Process payments)
- ✅ InvoicesController (Generate invoices)
- ✅ HousekeepingController (Create tasks, Assign tasks)

### 👔 MANAGER
- ✅ **TẤT CẢ CONTROLLERS** của Receptionist +
- ✅ Hotels, RoomTypes, Rooms, Amenities (CRUD)
- ✅ PromotionsController (CRUD)
- ✅ RatePlansController (CRUD)
- ✅ UsersController (Create Receptionist/Housekeeping)
- ✅ HousekeepingController (View all, Delete)
- ✅ ReportsController (All reports)
- ✅ PaymentsController (Refunds)

### 👨‍💼 ADMIN
- ✅ **TẤT CẢ CONTROLLERS** +
- ✅ UsersController (Create all roles, Delete users)

---

## WORKFLOW VÍ DỤ

### 📖 Quy trình đặt phòng (Customer):
1. Customer xem Hotels → RoomTypes → Rooms (Available)
2. Customer kiểm tra RatePlans → chọn gói giá tốt nhất
3. Customer validate Promotion code (nếu có)
4. Customer tạo Booking → Tự động tạo Guest record
5. Customer xem Invoice và thanh toán

### 🏨 Quy trình check-in (Receptionist):
1. Receptionist tìm Booking
2. Receptionist verify Guest information
3. Receptionist thực hiện Check-in
4. Receptionist assign Room
5. Hệ thống update Room status → "Occupied"

### 🧹 Quy trình dọn phòng (Housekeeping):
1. Customer check-out → Tự động tạo Housekeeping Task
2. Room status → "Maintenance"
3. Housekeeping staff claim Task
4. Housekeeping complete Task → Update status "Completed"
5. Room status tự động → "Available"

### 📊 Báo cáo (Manager):
1. Manager truy cập Reports
2. Xem Revenue, Occupancy, Performance
3. Export PDF để phân tích

---

## TÓM TẮT

✅ **14 Controllers** hoàn chỉnh
✅ **5 Vai trò** được phân quyền rõ ràng
✅ **Role-based Authorization** trên tất cả endpoints
✅ **RESTful API** standards
✅ **Consistent error handling**
✅ **DTO pattern** cho input/output

**Controllers mới thêm từ Đồ án 1:**
- ⭐ PromotionsController
- ⭐ RatePlansController

**Tất cả controllers đã được implement theo đúng kiến trúc và phân quyền phù hợp!**
