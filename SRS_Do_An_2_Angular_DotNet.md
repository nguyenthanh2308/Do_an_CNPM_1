# Software Requirements Specification (SRS)
## Hệ Thống Quản Lý Khách Sạn - Đồ Án 2
### Frontend (Angular) + Backend (.NET Web API)

---

**Phiên bản:** 2.0  
**Ngày:** 26/01/2026  
**Kiến trúc:** Single Backend API + Angular Frontend

---

## 1. GIỚI THIỆU

### 1.1 Mục Đích
Tài liệu này mô tả đặc tả yêu cầu cho hệ thống quản lý khách sạn phiên bản 2, được thiết kế với **Frontend (Angular SPA)** tách biệt hoàn toàn khỏi **Backend (.NET Core Web API)**.

### 1.2 Phạm Vi Dự Án
- **Tên dự án:** Hotel Management System v2
- **Mục tiêu:**
  - Tách biệt Frontend và Backend thành 2 projects độc lập
  - Frontend: Angular SPA với TypeScript
  - Backend: .NET 8 Web API với Clean Architecture
  - Authentication: JWT Token-based
  - Bổ sung UI cho 3 vai trò: Manager, Receptionist, Housekeeping
  - RESTful API design

### 1.3 So Sánh với Đồ Án 1

| **Tiêu chí** | **Đồ Án 1** | **Đồ Án 2** |
|-------------|-------------|-------------|
| Kiến trúc | Monolithic MVC | Separated FE/BE |
| Frontend | Razor Views (Server-side) | Angular 17+ SPA |
| Backend | ASP.NET Core MVC | ASP.NET Core Web API |
| Authentication | Cookie-based | JWT Token |
| Database | MySQL | MySQL (giữ nguyên) |
| Communication | N/A (tích hợp) | REST API (HTTP/JSON) |
| Deployment | Single app | 2 apps (FE + BE) |
| User Roles | Admin | Manager, Receptionist, Housekeeping, Customer |

---

## 2. KIẾN TRÚC HỆ THỐNG

### 2.1 Tổng Quan Kiến Trúc

```
┌──────────────────────────────────────────────────┐
│           Angular Frontend (SPA)                 │
│                Port 4200                         │
│  ┌────────────────────────────────────────────┐ │
│  │ - Manager Dashboard                        │ │
│  │ - Receptionist Dashboard                   │ │
│  │ - Housekeeping Dashboard                   │ │
│  │ - Customer Portal                          │ │
│  └────────────────────────────────────────────┘ │
└────────────────────┬─────────────────────────────┘
                     │
                     │ HTTP/JSON
                     │ JWT Token
                     │
┌────────────────────▼─────────────────────────────┐
│        .NET Core Web API (Backend)               │
│                Port 5000                         │
│  ┌────────────────────────────────────────────┐ │
│  │ Controllers Layer                          │ │
│  │ - AuthController                           │ │
│  │ - BookingController                        │ │
│  │ - RoomController                           │ │
│  │ - PaymentController                        │ │
│  │ - HousekeepingController                   │ │
│  └────────────────┬───────────────────────────┘ │
│  ┌────────────────▼───────────────────────────┐ │
│  │ Business Logic Layer (Services)            │ │
│  └────────────────┬───────────────────────────┘ │
│  ┌────────────────▼───────────────────────────┐ │
│  │ Data Access Layer (Repositories)           │ │
│  └────────────────┬───────────────────────────┘ │
└────────────────────┼─────────────────────────────┘
                     │
                     │ Entity Framework Core
                     │
┌────────────────────▼─────────────────────────────┐
│              MySQL Database                      │
│           hotel_management_db                    │
└──────────────────────────────────────────────────┘
```

### 2.2 Phân Tầng Backend (.NET Web API)

#### **Presentation Layer (Controllers)**
- Xử lý HTTP requests/responses
- Validation đầu vào
- Trả về JSON responses
- JWT authentication

#### **Business Logic Layer (Services)**
- Chứa business rules
- Service interfaces và implementations
- DTOs (Data Transfer Objects)

#### **Data Access Layer (Repositories)**
- Repository Pattern
- Entity Framework Core
- Database context
- Migrations

#### **Domain Layer (Models)**
- Entity models
- Domain logic
- Value objects

---

## 3. CHI TIẾT BACKEND API

### 3.1 Technology Stack

**Framework & Libraries:**
- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- Pomelo.EntityFrameworkCore.MySql
- JWT Bearer Authentication
- AutoMapper (DTO mapping)
- FluentValidation
- Swagger/OpenAPI

**Architecture Pattern:**
- Clean Architecture / Onion Architecture
- Repository Pattern
- Dependency Injection
- CQRS (optional, cho advanced features)

### 3.2 Project Structure

```
HotelManagement.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── BookingsController.cs
│   ├── RoomsController.cs
│   ├── RoomTypesController.cs
│   ├── GuestsController.cs
│   ├── PaymentsController.cs
│   ├── HousekeepingController.cs
│   └── ReportsController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IBookingService.cs
│   │   ├── IRoomService.cs
│   │   └── ...
│   └── Implementations/
│       ├── AuthService.cs
│       ├── BookingService.cs
│       └── ...
├── Repositories/
│   ├── Interfaces/
│   │   ├── IBookingRepository.cs
│   │   └── ...
│   └── Implementations/
│       ├── BookingRepository.cs
│       └── ...
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Booking.cs
│   │   ├── Room.cs
│   │   └── ...
│   └── DTOs/
│       ├── BookingDto.cs
│       ├── RoomDto.cs
│       └── ...
├── Data/
│   ├── HotelDbContext.cs
│   └── Migrations/
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── JwtMiddleware.cs
└── Program.cs
```

### 3.3 REST API Endpoints

#### **Authentication**
```
POST   /api/auth/login             # Đăng nhập
POST   /api/auth/register          # Đăng ký (Customer)
POST   /api/auth/refresh-token     # Refresh JWT token
GET    /api/auth/me                # Lấy thông tin user hiện tại
```

#### **Users (Manager only)**
```
GET    /api/users                  # Danh sách users
GET    /api/users/{id}             # Chi tiết user
POST   /api/users                  # Tạo user mới
PUT    /api/users/{id}             # Cập nhật user
DELETE /api/users/{id}             # Xóa user
```

#### **Rooms**
```
GET    /api/rooms                  # Danh sách phòng
GET    /api/rooms/{id}             # Chi tiết phòng
POST   /api/rooms                  # Tạo phòng (Manager)
PUT    /api/rooms/{id}             # Cập nhật phòng (Manager)
DELETE /api/rooms/{id}             # Xóa phòng (Manager)
GET    /api/rooms/available        # Phòng trống theo ngày
```

#### **Room Types**
```
GET    /api/room-types             # Danh sách loại phòng
GET    /api/room-types/{id}        # Chi tiết loại phòng
POST   /api/room-types             # Tạo loại phòng (Manager)
PUT    /api/room-types/{id}        # Cập nhật loại phòng (Manager)
```

#### **Bookings**
```
GET    /api/bookings               # Danh sách bookings
GET    /api/bookings/{id}          # Chi tiết booking
POST   /api/bookings               # Tạo booking
PUT    /api/bookings/{id}          # Cập nhật booking
DELETE /api/bookings/{id}          # Hủy booking
POST   /api/bookings/{id}/checkin  # Check-in
POST   /api/bookings/{id}/checkout # Check-out
GET    /api/bookings/my-bookings   # Bookings của customer
```

#### **Guests**
```
GET    /api/guests                 # Danh sách khách
GET    /api/guests/{id}            # Chi tiết khách
POST   /api/guests                 # Tạo khách
PUT    /api/guests/{id}            # Cập nhật khách
```

#### **Payments**
```
POST   /api/payments               # Xử lý thanh toán
GET    /api/payments/{bookingId}   # Chi tiết payment
GET    /api/invoices/{bookingId}   # Lấy hóa đơn
```

#### **Housekeeping**
```
GET    /api/housekeeping/tasks     # Danh sách task
GET    /api/housekeeping/tasks/{id}# Chi tiết task
PUT    /api/housekeeping/tasks/{id}# Cập nhật trạng thái
POST   /api/housekeeping/issues    # Báo cáo sự cố
```

#### **Reports (Manager only)**
```
GET    /api/reports/revenue        # Báo cáo doanh thu
GET    /api/reports/occupancy      # Tỷ lệ lấp đầy
GET    /api/reports/popular-rooms  # Phòng được đặt nhiều
```

### 3.4 API Response Format

**Success Response:**
```
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully"
}
```

**Error Response:**
```
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "checkInDate",
      "message": "Check-in date cannot be in the past"
    }
  ]
}
```

**Pagination Response:**
```
{
  "success": true,
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalRecords": 48
  }
}
```

---

## 4. AUTHENTICATION & AUTHORIZATION

### 4.1 JWT Authentication Flow

1. User gửi credentials (username/password) → `/api/auth/login`
2. Backend validate credentials
3. Nếu hợp lệ, tạo JWT token (access token + refresh token)
4. Frontend lưu token vào localStorage/sessionStorage
5. Mọi request tiếp theo đính kèm token: `Authorization: Bearer {token}`
6. Backend validate token và extracts user claims (userId, role)

### 4.2 JWT Token Structure

**Access Token (Expiration: 1 hour):**
- sub: User ID
- email: User email
- name: Full name
- role: Manager / Receptionist / Housekeeping / Customer
- exp: Expiration timestamp
- iat: Issued at timestamp

**Refresh Token (Expiration: 7 days):**
- Lưu trong database
- Dùng để gia hạn access token khi hết hạn

### 4.3 Role-Based Access Control

| Endpoint | Manager | Receptionist | Housekeeping | Customer |
|----------|---------|--------------|--------------|----------|
| GET /api/users | ✅ | ❌ | ❌ | ❌ |
| POST /api/bookings | ✅ | ✅ | ❌ | ✅ |
| POST /api/rooms | ✅ | ❌ | ❌ | ❌ |
| GET /api/housekeeping/tasks | ✅ | ✅ | ✅ | ❌ |
| PUT /api/housekeeping/tasks | ✅ | ❌ | ✅ | ❌ |
| GET /api/reports | ✅ | ❌ | ❌ | ❌ |

---

## 5. DATABASE DESIGN

### 5.1 Nguyên Tắc
- Sử dụng 1 database duy nhất: `hotel_management_db`
- Giữ nguyên schema từ Đồ án 1
- Thêm bảng RefreshTokens cho JWT

### 5.2 Database Tables

#### **Users**
- UserId (PK)
- Username, Email (Unique)
- PasswordHash (BCrypt)
- FullName, PhoneNumber
- Role: Manager, Receptionist, Housekeeping, Customer
- IsActive, CreatedAt, UpdatedAt

#### **RefreshTokens** (Mới)
- TokenId (PK)
- UserId (FK → Users)
- Token (hashed)
- ExpiresAt
- CreatedAt

#### **Bookings**
- BookingId (PK)
- GuestId (FK → Guests)
- UserId (FK → Users - người tạo booking)
- CheckInDate, CheckOutDate
- TotalAmount, DiscountAmount
- Status: Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
- CreatedAt, UpdatedAt

#### **Rooms**
- RoomId (PK)
- RoomNumber (Unique)
- RoomTypeId (FK → RoomTypes)
- Floor
- Status: Available, Occupied, Cleaning, Maintenance

#### **RoomTypes**
- RoomTypeId (PK)
- Name, Description
- BasePrice, Capacity
- ImageUrl

#### **Guests**
- GuestId (PK)
- UserId (FK → Users, nullable)
- FullName, Email, PhoneNumber
- Address, IdentityNumber
- CreatedAt

#### **Payments**
- PaymentId (PK)
- BookingId (FK → Bookings)
- Amount
- PaymentMethod: Cash, CreditCard, DebitCard, Transfer
- Status: Pending, Completed, Failed, Refunded
- PaymentDate

#### **HousekeepingTasks**
- TaskId (PK)
- RoomId (FK → Rooms)
- AssignedTo (FK → Users)
- TaskType: Cleaning, Maintenance, Inspection
- Status: Pending, InProgress, Completed
- Priority: Low, Medium, High
- DueDate, CompletedAt

*(Các bảng khác giữ nguyên: Amenities, RoomTypeAmenities, Promotions, RatePlans, Invoices)*

---

## 6. FRONTEND ARCHITECTURE (ANGULAR)

### 6.1 Technology Stack

**Core:**
- Angular 17+ (Standalone Components)
- TypeScript 5+
- RxJS
- Angular Router

**UI Framework:**
- Angular Material / PrimeNG / Bootstrap
- Tailwind CSS (optional)

**State Management:**
- RxJS BehaviorSubject
- NgRx (optional, cho complex state)

**HTTP Communication:**
- HttpClient
- Interceptors (JWT, Error handling)

### 6.2 Project Structure

```
hotel-management-frontend/
├── src/
│   ├── app/
│   │   ├── core/                    # Singleton services
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── jwt.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── api.service.ts
│   │   │   │   └── storage.service.ts
│   │   │   └── models/
│   │   │       ├── user.model.ts
│   │   │       ├── booking.model.ts
│   │   │       └── room.model.ts
│   │   │
│   │   ├── shared/                  # Reusable components
│   │   │   ├── components/
│   │   │   │   ├── navbar/
│   │   │   │   ├── sidebar/
│   │   │   │   ├── loading-spinner/
│   │   │   │   └── confirm-dialog/
│   │   │   └── pipes/
│   │   │       ├── date-format.pipe.ts
│   │   │       └── currency-format.pipe.ts
│   │   │
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   └── register/
│   │   │   │
│   │   │   ├── manager/
│   │   │   │   ├── dashboard/
│   │   │   │   ├── users/
│   │   │   │   ├── reports/
│   │   │   │   ├── rooms/
│   │   │   │   └── promotions/
│   │   │   │
│   │   │   ├── receptionist/
│   │   │   │   ├── bookings/
│   │   │   │   ├── checkin-checkout/
│   │   │   │   └── guests/
│   │   │   │
│   │   │   ├── housekeeping/
│   │   │   │   ├── tasks/
│   │   │   │   └── room-status/
│   │   │   │
│   │   │   └── customer/
│   │   │       ├── search-rooms/
│   │   │       ├── my-bookings/
│   │   │       └── profile/
│   │   │
│   │   ├── app.routes.ts
│   │   └── app.component.ts
│   │
│   ├── assets/
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   └── styles.scss
│
├── angular.json
├── package.json
└── tsconfig.json
```

### 6.3 Key Services

#### **AuthService**
```typescript
class AuthService {
  login(credentials)
  register(userData)
  logout()
  refreshToken()
  getCurrentUser()
  isAuthenticated()
  hasRole(role)
}
```

#### **ApiService** (Base HTTP Service)
```typescript
class ApiService {
  get<T>(endpoint)
  post<T>(endpoint, body)
  put<T>(endpoint, body)
  delete<T>(endpoint)
}
```

#### **BookingService**
```typescript
class BookingService {
  getBookings(filters)
  getBookingById(id)
  createBooking(booking)
  updateBooking(id, booking)
  cancelBooking(id)
  checkIn(id)
  checkOut(id)
}
```

### 6.4 Routing & Guards

**Routes:**
```
/login                    # Public
/register                 # Public
/manager/dashboard        # Manager only
/manager/users            # Manager only
/manager/reports          # Manager only
/receptionist/bookings    # Receptionist + Manager
/receptionist/checkin     # Receptionist + Manager
/housekeeping/tasks       # Housekeeping + Manager
/customer/search          # Customer
/customer/bookings        # Customer
```

**Guards:**
- `AuthGuard`: Kiểm tra đã login chưa
- `RoleGuard`: Kiểm tra role phù hợp

### 6.5 UI Dashboard Features

#### **Manager Dashboard**
- **Overview Cards:**
  - Doanh thu hôm nay / tháng
  - Số booking hôm nay
  - Tỷ lệ lấp đầy (%)
  - Phòng trống
- **Charts:**
  - Revenue trend (line chart)
  - Room type popularity (pie chart)
  - Booking status breakdown
- **Quick Actions:**
  - Xem tất cả bookings
  - Quản lý users
  - Xem reports

#### **Receptionist Dashboard**
- **Booking Calendar:** Fullcalendar view
- **Today's Arrivals/Departures**
- **Quick Check-in/Check-out**
- **Guest Search**

#### **Housekeeping Dashboard**
- **Task Board:** Kanban-style (Pending → InProgress → Completed)
- **Room Status Grid:** Visual floor plan
- **Priority Tasks:** Highlighted

---

## 7. DEVELOPMENT WORKFLOW

### 7.1 Migration Strategy

**Phase 1: Backend Setup (Week 1-2)**
1. Tạo project .NET Web API mới
2. Copy entities từ Đồ án 1
3. Implement Repository pattern
4. Tạo Controllers và DTOs
5. Setup JWT authentication
6. Test APIs với Postman/Swagger

**Phase 2: Frontend Setup (Week 3-4)**
1. Tạo Angular project
2. Setup routing và guards
3. Tạo login/register pages
4. Implement AuthService
5. Test authentication flow

**Phase 3: Feature Implementation (Week 5-8)**
1. Manager features
2. Receptionist features
3. Housekeeping features
4. Customer features

**Phase 4: Integration & Testing (Week 9-10)**
1. Connect all FE-BE endpoints
2. Testing
3. Bug fixing
4. Documentation

### 7.2 Testing Strategy

**Backend:**
- Unit tests (xUnit)
- Integration tests (WebApplicationFactory)
- API tests (Postman collections)

**Frontend:**
- Unit tests (Jasmine/Karma)
- Component tests
- E2E tests (Cypress)

---

## 8. DEPLOYMENT

### 8.1 Development Environment

**Backend:**
- Run: `dotnet run` (Port 5000)
- Database: MySQL local

**Frontend:**
- Run: `ng serve` (Port 4200)
- API URL: `http://localhost:5000`

### 8.2 Production Environment

**Backend:**
- Deploy to: Azure App Service / AWS / VPS
- Database: Managed MySQL (Azure/AWS)

**Frontend:**
- Build: `ng build --prod`
- Deploy to: Netlify / Vercel / Azure Static Web Apps
- Environment config: Production API URL

---

## 9. SECURITY CONSIDERATIONS

1. **HTTPS Only** - Production phải dùng HTTPS
2. **CORS** - Configure CORS cho phép Frontend domain
3. **Input Validation** - FluentValidation ở Backend
4. **SQL Injection Prevention** - Dùng EF Core (parameterized queries)
5. **XSS Prevention** - Angular sanitizes by default
6. **Password Security** - BCrypt với cost factor ≥ 12
7. **JWT Security** - Secret key mạnh, short expiration time
8. **Rate Limiting** - Prevent brute force attacks

---

## 10. YÊU CẦU PHI CHỨC NĂNG

**Performance:**
- API response time < 200ms
- Frontend initial load < 3s
- Support 500 concurrent users

**Scalability:**
- Horizontal scaling cho API (load balancer)
- Database connection pooling

**Availability:**
- Uptime 99%
- Auto-restart on crashes

**Usability:**
- Responsive design (mobile-friendly)
- Accessible (WCAG 2.1 Level AA)

---

## PHỤ LỤC

### A. Technology Versions
- .NET: 8.0
- Angular: 17+
- MySQL: 8.0
- Node.js: 18+ (cho Angular development)

### B. Development Tools
- Backend: Visual Studio 2022 / Rider / VS Code
- Frontend: VS Code
- API Testing: Postman
- Database: MySQL Workbench
- Version Control: Git + GitHub

---

**KẾT THÚC SRS - Đồ Án 2**

*Document Version: 2.1 - Simplified Architecture*  
*Last Updated: 26/01/2026*
