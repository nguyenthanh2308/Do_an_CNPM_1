# Software Requirements Specification (SRS)
## Hệ Thống Quản Lý Khách Sạn - Đồ Án 2
### Kiến Trúc Microservices

---

**Phiên bản:** 2.0  
**Ngày:** 26/01/2026  
**Nhóm sinh viên:** [Tên nhóm]  
**Giảng viên hướng dẫn:** [Tên giảng viên]

---

## 1. GIỚI THIỆU

### 1.1 Mục Đích
Tài liệu này mô tả đặc tả yêu cầu phần mềm cho hệ thống quản lý khách sạn phiên bản 2, được thiết kế theo kiến trúc Microservices với Frontend (Angular) tách biệt khỏi Backend (.NET Core Web API).

### 1.2 Phạm Vi Dự Án
- **Tên dự án:** Hotel Management System v2
- **Mục tiêu chính:**
  - Tách biệt Frontend và Backend
  - Chuyển đổi từ Monolithic sang Microservices
  - Bổ sung 3 vai trò mới: Manager, Receptionist, Housekeeping
  - Áp dụng các công nghệ hiện đại: API Gateway, Message Queue, Caching

### 1.3 So Sánh với Đồ Án 1

| **Tiêu chí** | **Đồ Án 1** | **Đồ Án 2** |
|-------------|-------------|-------------|
| Kiến trúc | Monolithic | Microservices |
| Frontend | Razor Views | Angular SPA |
| Backend | ASP.NET Core MVC | ASP.NET Core Web API |
| Authentication | Cookie-based | JWT Token |
| Database | Single MySQL | Multiple databases |
| Communication | Direct calls | REST + Message Queue |
| Deployment | Single app | Containerized services |

### 1.4 Tài Liệu Tham Khảo
- Microsoft Microservices eBook
- ASP.NET Core Web API Documentation
- Angular Official Documentation
- RabbitMQ Tutorials
- Ocelot API Gateway Documentation

---

## 2. MÔ TẢ TỔNG QUÁT

### 2.1 Bối Cảnh Hệ Thống
Hệ thống được thiết kế để quản lý toàn bộ quy trình vận hành khách sạn, từ đặt phòng, check-in/out, thanh toán, đến quản lý dọn phòng và báo cáo.

### 2.2 Chức Năng Chính
- Quản lý đặt phòng (Booking)
- Quản lý phòng và loại phòng
- Quản lý khách hàng
- Xử lý thanh toán
- Quản lý công việc dọn phòng
- Quản lý nhân viên
- Báo cáo và thống kê
- Hệ thống thông báo

### 2.3 Người Dùng và Vai Trò

| **Vai trò** | **Mô tả** | **Quyền hạn chính** |
|------------|-----------|---------------------|
| **Manager** | Quản lý khách sạn | Dashboard, Báo cáo, Quản lý nhân viên, Thiết lập giá |
| **Receptionist** | Lễ tân | Booking CRUD, Check-in/out, Quản lý khách |
| **Housekeeping** | Nhân viên dọn phòng | Xem task, Cập nhật trạng thái phòng |
| **Customer** | Khách hàng | Đặt phòng, Xem lịch sử |

---

## 3. KIẾN TRÚC HỆ THỐNG

### 3.1 Tổng Quan Kiến Trúc

```
┌─────────────────────────────────────────────────────────┐
│                    Angular Frontend                      │
│                        (Port 4200)                       │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                   API Gateway (Ocelot)                   │
│                        (Port 5000)                       │
└──┬────┬────┬────┬────┬────┬────┬──────────────────────┘
   │    │    │    │    │    │    │
   ▼    ▼    ▼    ▼    ▼    ▼    ▼
┌──────────────────────────────────────────────────────────┐
│                   Microservices Layer                     │
├───────────┬────────────┬──────────┬─────────────────────┤
│ Auth      │ Booking    │ Room     │ Payment             │
│ :5001     │ :5002      │ :5003    │ :5004               │
├───────────┼────────────┼──────────┼─────────────────────┤
│ Customer  │ Housekeep  │ Notif    │                     │
│ :5005     │ :5006      │ :5007    │                     │
└───────────┴────────────┴──────────┴─────────────────────┘
         │                   │
         ▼                   ▼
┌─────────────────┐   ┌──────────────┐
│  MySQL (x6 DB)  │   │ Redis Cache  │
└─────────────────┘   └──────────────┘
         │
         ▼
   ┌──────────────┐
   │  RabbitMQ    │
   └──────────────┘
```

### 3.2 Microservices Chi Tiết

#### 3.2.1 Auth Service (Port 5001)
**Chức năng:**
- JWT authentication (login/logout/refresh token)
- User management (CRUD)
- Role-based access control

**Database:** `hotel_auth_db`
**Tables:** Users, RefreshTokens

**APIs:**
```
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh-token
GET    /api/users/{id}
PUT    /api/users/{id}
```

#### 3.2.2 Booking Service (Port 5002)
**Chức năng:**
- Quản lý booking (CRUD)
- Kiểm tra phòng trống
- Áp dụng promotion
- Check-in / Check-out

**Database:** `hotel_booking_db`
**Tables:** Bookings, BookingRooms, Promotions, RatePlans

**Events Published:**
- `BookingCreated` → Payment, Notification
- `CheckOutCompleted` → Housekeeping, Payment

**APIs:**
```
POST   /api/bookings
GET    /api/bookings/{id}
PUT    /api/bookings/{id}
DELETE /api/bookings/{id}
GET    /api/bookings/availability
POST   /api/bookings/{id}/check-in
```

#### 3.2.3 Room Service (Port 5003)
**Chức năng:**
- Quản lý phòng và loại phòng
- Quản lý tiện nghi
- Cập nhật trạng thái phòng

**Database:** `hotel_room_db`
**Tables:** Rooms, RoomTypes, Amenities, RoomTypeAmenities

**Caching:** Redis (TTL: 1 hour cho RoomTypes & Amenities)

#### 3.2.4 Payment Service (Port 5004)
**Chức năng:**
- Xử lý thanh toán
- Tạo hóa đơn
- Hoàn tiền

**Database:** `hotel_payment_db`
**Tables:** Payments, Invoices

**Events Subscribed:**
- `BookingCreated` → Generate invoice

#### 3.2.5 Customer Service (Port 5005)
**Chức năng:**
- Quản lý thông tin khách
- Lịch sử booking

**Database:** `hotel_customer_db`
**Tables:** Guests

#### 3.2.6 Housekeeping Service (Port 5006) - MỚI
**Chức năng:**
- Quản lý task dọn phòng
- Báo cáo sự cố

**Database:** `hotel_housekeeping_db`
**Tables:** HousekeepingTasks

**Events Subscribed:**
- `CheckOutCompleted` → Auto-create cleaning task

#### 3.2.7 Notification Service (Port 5007)
**Chức năng:**
- Gửi email thông báo
- SMS (tương lai)

**Events Subscribed:**
- `BookingCreated` → Send confirmation email

---

## 4. THIẾT KẾ CƠ SỞ DỮ LIỆU

### 4.1 Nguyên Tắc Database
- Mỗi microservice có database riêng biệt
- Eventual consistency thay vì strong consistency
- Sử dụng events để đồng bộ dữ liệu giữa services

### 4.2 Auth Database (`hotel_auth_db`)

**Users Table:**
- UserId (PK)
- Username, Email (Unique)
- PasswordHash (BCrypt)
- FullName, PhoneNumber
- Role: Manager, Receptionist, Housekeeping, Customer
- IsActive, CreatedAt, UpdatedAt

**RefreshTokens Table:**
- TokenId (PK)
- UserId (FK → Users)
- Token
- ExpiresAt, CreatedAt

### 4.3 Booking Database (`hotel_booking_db`)

**Bookings Table:**
- BookingId (PK)
- GuestId (Reference to Customer Service)
- UserId (Reference to Auth Service)
- CheckInDate, CheckOutDate
- TotalAmount, DiscountAmount
- Status: Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
- CreatedAt

**BookingRooms Table:**
- BookingRoomId (PK)
- BookingId (FK → Bookings)
- RoomId (Reference to Room Service)
- RoomTypeId (Denormalized từ Room Service)
- PricePerNight

**Promotions Table:**
- PromotionId (PK)
- Code (Unique)
- DiscountPercentage
- ValidFrom, ValidTo
- IsActive

**RatePlans Table:**
- RatePlanId (PK)
- Name
- RoomTypeId (Reference to Room Service)
- PricePerNight
- ValidFrom, ValidTo
- IsActive

### 4.4 Room Database (`hotel_room_db`)

**RoomTypes Table:**
- RoomTypeId (PK)
- Name, Description
- BasePrice
- Capacity
- ImageUrl

**Rooms Table:**
- RoomId (PK)
- RoomNumber (Unique)
- RoomTypeId (FK → RoomTypes)
- Floor
- Status: Available, Occupied, Cleaning, Maintenance

**Amenities Table:**
- AmenityId (PK)
- Name
- IconClass

**RoomTypeAmenities Table (Mapping):**
- RoomTypeId (FK → RoomTypes)
- AmenityId (FK → Amenities)

### 4.5 Payment Database (`hotel_payment_db`)

**Payments Table:**
- PaymentId (PK)
- BookingId (Reference to Booking Service)
- Amount
- PaymentMethod: Cash, CreditCard, DebitCard, Transfer
- Status: Pending, Completed, Failed, Refunded
- TransactionId
- PaymentDate

**Invoices Table:**
- InvoiceId (PK)
- BookingId (Reference to Booking Service)
- InvoiceNumber (Unique)
- TotalAmount, TaxAmount
- IssueDate
- PdfUrl

### 4.6 Customer Database (`hotel_customer_db`)

**Guests Table:**
- GuestId (PK)
- UserId (Optional link to Auth Service)
- FullName, Email, PhoneNumber
- Address
- IdentityNumber
- CreatedAt

### 4.7 Housekeeping Database (`hotel_housekeeping_db`)

**HousekeepingTasks Table:**
- TaskId (PK)
- RoomId (Reference to Room Service)
- AssignedTo (UserId từ Auth Service)
- TaskType: Cleaning, Maintenance, Inspection
- Status: Pending, InProgress, Completed
- Priority: Low, Medium, High
- Notes
- DueDate, CompletedAt, CreatedAt

---

## 5. XÁC THỰC VÀ PHÂN QUYỀN

### 5.1 JWT Authentication Flow

1. User login → Auth Service validate credentials
2. Auth Service generate JWT (access token) + Refresh token
3. Store refresh token in Redis
4. Return tokens to client
5. Client attach access token in header: `Authorization: Bearer {token}`
6. API Gateway validate JWT signature
7. Forward request to microservice with user claims

### 5.2 JWT Token Structure
**Claims trong token:**
- sub: User ID
- email: Email người dùng
- name: Tên đầy đủ
- role: Manager / Receptionist / Housekeeping / Customer
- exp: Expiration time (1 giờ)
- iat: Issued at time
- iss: Issuer (HotelAPI)
- aud: Audience (HotelClient)

### 5.3 Role Permissions

| Vai trò | Dashboard | Booking | Room Mgmt | User Mgmt | Reports | Housekeeping |
|---------|-----------|---------|-----------|-----------|---------|--------------|
| Manager | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Receptionist | ❌ | ✅ | View only | ❌ | ❌ | View |
| Housekeeping | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Customer | ❌ | Own only | ❌ | ❌ | ❌ | ❌ |

---

## 6. API DESIGN

### 6.1 API Gateway (Ocelot)
**Base URL:** `http://localhost:5000`

**Chức năng:**
- Routing requests đến các microservices phù hợp
- JWT token validation
- Rate limiting
- Load balancing

**Route Configuration:**
- `/api/auth/*` → Auth Service (port 5001)
- `/api/bookings/*` → Booking Service (port 5002)
- `/api/rooms/*` → Room Service (port 5003)
- `/api/payments/*` → Payment Service (port 5004)
- `/api/guests/*` → Customer Service (port 5005)
- `/api/housekeeping/*` → Housekeeping Service (port 5006)
- `/api/notifications/*` → Notification Service (port 5007)

### 6.2 API Response Format

**Success Response:**
- success: true
- data: object chứa dữ liệu
- message: thông báo thành công
- errors: null

**Error Response:**
- success: false
- data: null
- message: mô tả lỗi
- errors: array chứa chi tiết lỗi từng field

### 6.3 Core API Endpoints

#### Authentication
```
POST   /api/auth/login              # Login
POST   /api/auth/register           # Register
POST   /api/auth/refresh-token      # Refresh token
GET    /api/users                   # List users (Manager only)
```

#### Booking
```
GET    /api/bookings                # List bookings
POST   /api/bookings                # Create booking
GET    /api/bookings/{id}           # Get booking details
PUT    /api/bookings/{id}           # Update booking
DELETE /api/bookings/{id}           # Cancel booking
GET    /api/bookings/availability   # Check availability
POST   /api/bookings/{id}/check-in  # Check-in
POST   /api/bookings/{id}/check-out # Check-out
```

#### Rooms
```
GET    /api/rooms                   # List all rooms
GET    /api/rooms/{id}              # Get room details
POST   /api/rooms                   # Create room (Manager)
PUT    /api/rooms/{id}              # Update room (Manager)
GET    /api/room-types              # List room types
```

#### Housekeeping
```
GET    /api/housekeeping/tasks      # My tasks
PUT    /api/housekeeping/tasks/{id} # Update task status
POST   /api/housekeeping/issues     # Report issue
```

---

## 7. FRONTEND ARCHITECTURE (ANGULAR)

### 7.1 Project Structure
```
hotel-management-frontend/
├── src/app/
│   ├── core/                   # Singleton services
│   │   ├── guards/
│   │   ├── interceptors/
│   │   └── services/
│   ├── shared/                 # Reusable components
│   │   ├── components/
│   │   └── models/
│   └── features/
│       ├── manager/            # Manager dashboard
│       ├── receptionist/       # Receptionist dashboard
│       ├── housekeeping/       # Housekeeping dashboard
│       └── customer/           # Customer portal
```

### 7.2 Dashboard Features

**Manager Dashboard:**
- Tổng quan doanh thu, tỷ lệ lấp đầy
- Quản lý nhân viên (CRUD users)
- Báo cáo và thống kê
- Thiết lập giá phòng, promotion

**Receptionist Dashboard:**
- Calendar view booking
- Check-in/Check-out
- Guest management
- Payment processing

**Housekeeping Dashboard:**
- Task list (Today, Overdue)
- Room status board
- Report issues

---

## 8. INFRASTRUCTURE

### 8.1 Message Queue (RabbitMQ)

**Events:**
- `BookingCreated`
- `BookingCancelled`
- `CheckInCompleted`
- `CheckOutCompleted`
- `PaymentCompleted`
- `RoomCleaningCompleted`

**Pattern:** Publish/Subscribe

---

## 9. MIGRATION STRATEGY

### 9.1 Strangler Fig Pattern
Dần dần chuyển từng service ra khỏi monolith, cho phép chạy song song.

### 9.2 Timeline (12 tuần)

| Week | Phase | Tasks |
|------|-------|-------|
| 1-2 | Preparation | Setup Docker, Create base projects |
| 3-4 | Auth Service | Migrate User, Implement JWT |
| 5-6 | Room Service | Migrate Room entities, Setup Redis |
| 7-8 | Booking Service | Migrate Booking, Setup RabbitMQ |
| 9 | Payment Service | Migrate Payment, Subscribe events |
| 10 | Customer & Housekeeping | New services from scratch |
| 11 | Notification | Email service, Event subscribers |
| 12 | Testing | Integration & Performance testing |

---

## 10. YÊU CẦU PHI CHỨC NĂNG

### 10.1 Performance
- API response time < 200ms (95th percentile)
- Support 1000 concurrent users
- Database query time < 100ms

### 10.2 Security
- HTTPS only
- JWT token expiration: 1 hour
- Refresh token expiration: 7 days
- Password: BCrypt hash (cost factor: 12)
- Rate limiting: 100 requests/minute per IP

### 10.3 Availability
- Uptime: 99.9%
- Auto-restart failed services
- Health check endpoints

### 10.4 Scalability
- Horizontal scaling per service
- Load balancing via API Gateway
- Database connection pooling

---

## 11. TESTING STRATEGY

### 11.1 Unit Testing
- xUnit for .NET services
- Jasmine/Karma for Angular
- Coverage target: 80%

### 11.2 Integration Testing
- Test inter-service communication
- Test event publishing/subscribing
- Test API Gateway routing

### 11.3 E2E Testing
- Cypress for critical user flows
- Test complete booking process
- Test authentication flow

---

## 12. RISKS & MITIGATION

| Risk | Impact | Mitigation |
|------|--------|------------|
| Data consistency | High | Use eventual consistency + Saga pattern |
| Network latency | Medium | Aggressive caching, Circuit breaker |
| Learning curve | Medium | Training, Documentation |
| Database migration | High | Thorough testing, Backup strategy |

---

## PHỤ LỤC

### A. Technology Stack
- **Backend:** ASP.NET Core 8.0 Web API
- **Frontend:** Angular 17+
- **Database:** MySQL 8.0
- **Cache:** Redis
- **Message Queue:** RabbitMQ
- **API Gateway:** Ocelot
- **Containerization:** Docker

### B. Development Tools
- Visual Studio 2022 / VS Code
- Postman (API testing)
- MySQL Workbench
- Docker Desktop
- Git

---

**Kết thúc SRS - Đồ Án 2**

*Document Version: 2.0*  
*Last Updated: 26/01/2026*
