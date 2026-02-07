# Phase 4: API Controllers Implementation Plan

## Goal
Implement role-based API Controllers for Hotel Management System with proper authorization, validation, and RESTful endpoints.

---

## User Roles & Access Matrix

| Role | Vietnamese | Key Responsibilities |
|------|-----------|---------------------|
| **Admin** | Quản trị hệ thống | Full system access, user management |
| **Manager** | Quản lý | Operations oversight, reports, configuration |
| **Receptionist** | Tiếp tân | Bookings, check-in/out, guests, payments |
| **Housekeeping** | Nhân viên dọn phòng | Task management, room status updates |
| **Customer** | Khách hàng | View rooms, create bookings, view own bookings |

---

## Controllers TODO List by Priority

### 🔥 Priority 1: Core Authentication & Booking Flow

#### ✅ 1.1 AuthController (`/api/auth`)
**Access**: Public (login/register), Authenticated (me, refresh)

**Endpoints**:
- `POST /api/auth/login` - Login with username/password
- `POST /api/auth/register` - Register new customer
- `POST /api/auth/refresh-token` - Refresh JWT token
- `GET /api/auth/me` - Get current user info
- `POST /api/auth/logout` - Logout (invalidate refresh token)

**Priority**: ⭐⭐⭐⭐⭐ (Must have - Required for all other endpoints)

---

#### ✅ 1.2 BookingsController (`/api/bookings`)
**Access**: All authenticated users (role-based filtering)

**Role-based Endpoints**:

**Customer**:
- `GET /api/bookings/my-bookings` - View own bookings
- `POST /api/bookings` - Create new booking (self)
- `GET /api/bookings/{id}` - View own booking details
- `PUT /api/bookings/{id}/cancel` - Cancel own booking

**Receptionist**:
- `GET /api/bookings` - List all bookings (with filters)
- `GET /api/bookings/{id}` - View any booking
- `POST /api/bookings` - Create booking for guest
- `PUT /api/bookings/{id}` - Update booking
- `POST /api/bookings/{id}/checkin` - Check-in guest (verify payment, confirm room assignment)
- `POST /api/bookings/{id}/checkout` - **Check-out guest** ⚡ **Auto-creates housekeeping task**

**Manager**:
- All Receptionist permissions +
- `DELETE /api/bookings/{id}` - Delete booking (admin only)
- `GET /api/bookings/statistics` - Booking statistics

**Priority**: ⭐⭐⭐⭐⭐ (Core business logic)

> [!IMPORTANT]
> **Auto-Task Creation**: When Receptionist performs checkout (`POST /bookings/{id}/checkout`), the system **automatically creates a Housekeeping Task** for that room with status "Pending". Housekeeping staff can then claim and complete the task.

---

#### ✅ 1.3 RoomsController (`/api/rooms`)
**Access**: Public (view), Manager (manage)

**Public/Customer**:
- `GET /api/rooms` - List all rooms
- `GET /api/rooms/available` - Get available rooms by dates
- `GET /api/rooms/{id}` - Get room details

**Receptionist**:
- All Public permissions +
- `PUT /api/rooms/{id}/status` - Update room status (Available, Occupied, Maintenance)

**Manager**:
- All Receptionist permissions +
- `POST /api/rooms` - Create new room
- `PUT /api/rooms/{id}` - Update room details
- `DELETE /api/rooms/{id}` - Delete room
- `GET /api/rooms/statistics` - Room statistics

**Priority**: ⭐⭐⭐⭐⭐ (Core inventory)

---

#### ✅ 1.4 GuestsController (`/api/guests`)
**Access**: Receptionist, Manager

**Receptionist & Manager**:
- `GET /api/guests` - List all guests (with pagination)
- `GET /api/guests/{id}` - Get guest details
- `GET /api/guests/search?identity={number}` - Search by CCCD/CMND
- `GET /api/guests/search?name={name}` - Search by name
- `POST /api/guests` - Create guest profile
- `PUT /api/guests/{id}` - Update guest info

**Manager**:
- `DELETE /api/guests/{id}` - Delete guest (if no bookings)

**Priority**: ⭐⭐⭐⭐⭐ (Required for bookings)

---

### 🔥 Priority 2: Payment & Invoice

#### ✅ 2.1 PaymentsController (`/api/payments`)
**Access**: Receptionist, Manager

**Receptionist & Manager**:
- `POST /api/payments` - Process payment
- `GET /api/payments/booking/{bookingId}` - Get payments for booking
- `GET /api/payments/{id}` - Get payment details
- `POST /api/payments/{id}/refund` - Process refund

**Priority**: ⭐⭐⭐⭐ (Critical for revenue)

---

#### ✅ 2.2 InvoicesController (`/api/invoices`)
**Access**: Receptionist, Manager, Customer (own)

**Customer**:
- `GET /api/invoices/my-invoices` - Get own invoices
- `GET /api/invoices/{id}` - Get own invoice (if owner)
- `GET /api/invoices/{id}/download` - Download PDF (future)

**Receptionist & Manager**:
- `POST /api/invoices/generate/{bookingId}` - Generate invoice
- `GET /api/invoices` - List all invoices
- `GET /api/invoices/{id}` - Get any invoice
- `GET /api/invoices/booking/{bookingId}` - Get invoice by booking

**Priority**: ⭐⭐⭐⭐ (Legal requirement)

---

### 🔥 Priority 3: Hotel Configuration

#### ✅ 3.1 HotelsController (`/api/hotels`)
**Access**: Public (view), Manager (manage)

**Public/Customer**:
- `GET /api/hotels` - List hotels
- `GET /api/hotels/{id}` - Get hotel details
- `GET /api/hotels/{id}/details` - Get hotel with rooms & room types

**Manager**:
- `POST /api/hotels` - Create hotel
- `PUT /api/hotels/{id}` - Update hotel
- `DELETE /api/hotels/{id}` - Delete hotel (if no rooms)

**Priority**: ⭐⭐⭐ (Setup/Configuration)

---

#### ✅ 3.2 RoomTypesController (`/api/roomtypes`)
**Access**: Public (view), Manager (manage)

**Public/Customer**:
- `GET /api/roomtypes` - List room types
- `GET /api/roomtypes/{id}` - Get room type details
- `GET /api/roomtypes/{id}/amenities` - Get room type with amenities
- `GET /api/roomtypes/hotel/{hotelId}` - Get room types by hotel

**Manager**:
- `POST /api/roomtypes` - Create room type
- `PUT /api/roomtypes/{id}` - Update room type
- `DELETE /api/roomtypes/{id}` - Delete room type (if no rooms)

**Priority**: ⭐⭐⭐ (Setup/Configuration)

---

#### ✅ 3.3 AmenitiesController (`/api/amenities`)
**Access**: Public (view), Manager (manage)

**Public/Customer**:
- `GET /api/amenities` - List all amenities
- `GET /api/amenities/{id}` - Get amenity details
- `GET /api/amenities/roomtype/{id}` - Get amenities for room type

**Manager**:
- `POST /api/amenities` - Create amenity
- `PUT /api/amenities/{id}` - Update amenity
- `DELETE /api/amenities/{id}` - Delete amenity

**Priority**: ⭐⭐ (Supporting data)

---

### 🔥 Priority 4: User & Role Management

#### ✅ 4.1 UsersController (`/api/users`)
**Access**: Admin, Manager

**Manager**:
- `GET /api/users` - List all users (exclude Admin)
- `GET /api/users/{id}` - Get user details
- `POST /api/users` - Create user (Receptionist, Housekeeping only)
- `PUT /api/users/{id}` - Update user
- `PUT /api/users/{id}/deactivate` - Deactivate user

**Admin**:
- All Manager permissions +
- `DELETE /api/users/{id}` - Delete user
- Can create/manage all roles including Manager

**Priority**: ⭐⭐⭐ (Administrative)

---

### 🔥 Priority 5: Housekeeping Management

#### ✅ 5.1 HousekeepingController (`/api/housekeeping`)
**Access**: Receptionist (create), Housekeeping (manage own), Manager (oversee)

**Housekeeping Staff**:
- `GET /api/housekeeping/my-tasks` - Get tasks assigned to me
- `GET /api/housekeeping/tasks/pending` - Get all pending (unassigned) tasks
- `POST /api/housekeeping/tasks/{id}/claim` - **Claim/assign task to myself**
- `GET /api/housekeeping/tasks/{id}` - Get task details
- `PUT /api/housekeeping/tasks/{id}/status` - Update task status (InProgress → Completed)
- `PUT /api/housekeeping/tasks/{id}/notes` - Add task notes
- `POST /api/housekeeping/issues` - Report room issues

**Receptionist**:
- All Housekeeping permissions +
- `POST /api/housekeeping/tasks` - Manually create housekeeping task
- `PUT /api/housekeeping/tasks/{id}/assign` - Assign task to specific staff

**Manager**:
- All Receptionist permissions +
- `GET /api/housekeeping/tasks` - View all tasks (with filters)
- `DELETE /api/housekeeping/tasks/{id}` - Delete task

**Priority**: ⭐⭐⭐ (Operations)

> [!NOTE]
> **Housekeeping Workflow**:
> 1. Task auto-created on checkout (status: Pending, assigned: null)
> 2. Housekeeping staff views pending tasks: `GET /api/housekeeping/tasks/pending`
> 3. Staff claims task: `POST /api/housekeeping/tasks/{id}/claim` (auto-assigns to current user)
> 4. Staff starts work: `PUT /api/housekeeping/tasks/{id}/status` → "InProgress"
> 5. Staff completes: `PUT /api/housekeeping/tasks/{id}/status` → "Completed"
> 6. Room status auto-updated to "Available"

---

### 🔥 Priority 6: Reporting & Analytics

#### ✅ 6.1 ReportsController (`/api/reports`)
**Access**: Manager only

**Manager**:
- `GET /api/reports/revenue?from={date}&to={date}` - Revenue report
- `GET /api/reports/occupancy?date={date}` - Occupancy report
- `GET /api/reports/popular-rooms` - Most booked rooms
- `GET /api/reports/guest-statistics` - Guest demographics
- `GET /api/reports/payment-summary?from={date}&to={date}` - Payment analysis
- `GET /api/reports/export/revenue?format={pdf|excel}` - Export report (future)

**Priority**: ⭐⭐ (Business insights)

---

### 🔥 Priority 7: Advanced Features (Optional)

#### ⏰ 7.1 PromotionsController (`/api/promotions`)
**Access**: Public (view active), Manager (manage)

**Public/Customer**:
- `GET /api/promotions/active` - Get active promotions
- `GET /api/promotions/{id}` - Get promotion details
- `POST /api/promotions/validate` - Validate promo code

**Manager**:
- `GET /api/promotions` - List all promotions
- `POST /api/promotions` - Create promotion
- `PUT /api/promotions/{id}` - Update promotion
- `DELETE /api/promotions/{id}` - Delete promotion

**Priority**: ⭐ (Enhancement)

---

#### ⏰ 7.2 RatePlansController (`/api/rateplans`)
**Access**: Public (view), Manager (manage)

**Public/Customer**:
- `GET /api/rateplans/active` - Get active rate plans
- `GET /api/rateplans/roomtype/{id}` - Get rate plans for room type

**Manager**:
- `GET /api/rateplans` - List all rate plans
- `POST /api/rateplans` - Create rate plan
- `PUT /api/rateplans/{id}` - Update rate plan
- `DELETE /api/rateplans/{id}` - Delete rate plan

**Priority**: ⭐ (Enhancement)

---

#### ⏰ 7.3 NotificationsController (`/api/notifications`)
**Access**: Authenticated users

**All Authenticated**:
- `GET /api/notifications/my-notifications` - Get my notifications
- `GET /api/notifications/unread-count` - Get unread count
- `PUT /api/notifications/{id}/read` - Mark as read
- `PUT /api/notifications/mark-all-read` - Mark all as read

**Manager**:
- `POST /api/notifications/broadcast` - Send notification to all users

**Priority**: ⭐ (Nice to have)

---

## Implementation Order

### Phase 4A: Authentication & Core (Week 1)
1. ✅ **AuthController** - Enable login/register
2. ✅ **RoomsController** - Public endpoints (view, available)
3. ✅ **RoomTypesController** - Public endpoints (catalog)
4. ✅ **HotelsController** - Public endpoints (info)

**Goal**: Enable customer to browse and check availability

---

### Phase 4B: Booking Flow (Week 2)
5. ✅ **GuestsController** - Guest management
6. ✅ **BookingsController** - Create, view, cancel bookings
7. ✅ **PaymentsController** - Payment processing
8. ✅ **InvoicesController** - Invoice generation

**Goal**: Complete end-to-end booking flow

---

### Phase 4C: Management (Week 3)
9. ✅ **UsersController** - User management
10. ✅ **HousekeepingController** - Task management
11. ✅ **ReportsController** - Analytics
12. ✅ **AmenitiesController** - Configuration

**Goal**: Enable staff operations

---

### Phase 4D: Enhancements (Week 4 - Optional)
13. ⏰ **PromotionsController**
14. ⏰ **RatePlansController**
15. ⏰ **NotificationsController**

**Goal**: Advanced features

---

## Business Logic Requirements

### 🔄 Auto-Task Creation Workflow

**Trigger**: `POST /api/bookings/{id}/checkout`

**BookingService.CheckOutAsync() must**:
1. Validate booking exists and status is "CheckedIn"
2. Update booking status to "CheckedOut"
3. Generate invoice (via InvoiceService)
4. **Auto-create HousekeepingTask**:
   - TaskType: "RoomCleaning"
   - Status: "Pending"
   - AssignedUserId: null (unassigned)
   - RoomId: from booking
   - Priority: "Normal" (or "High" if next check-in is soon)
5. Update room status to "Maintenance" (being cleaned)
6. Return checkout response

**Code Implementation**:
```csharp
public async Task<BookingDto> CheckOutAsync(long bookingId)
{
    // ... validation & checkout logic ...
    
    // Auto-create housekeeping task
    var task = new HousekeepingTask
    {
        RoomId = booking.RoomId,
        TaskType = "RoomCleaning",
        Status = "Pending",
        AssignedUserId = null,
        Priority = "Normal",
        CreatedAt = DateTime.Now
    };
    await _unitOfWork.HousekeepingTasks.AddAsync(task);
    
    // Update room status
    room.Status = "Maintenance";
    _unitOfWork.Rooms.Update(room);
    
    await _unitOfWork.SaveChangesAsync();
    return mappedBooking;
}
```

---

### 👷 Housekeeping Task Claim Workflow

**Endpoint**: `POST /api/housekeeping/tasks/{id}/claim`

**HousekeepingController.ClaimTask() must**:
1. Verify task exists and status is "Pending"
2. Verify task is unassigned (AssignedUserId == null)
3. Assign task to current user (from JWT token)
4. Update status to "InProgress" (optional, or keep as Pending)
5. Return updated task

**Authorization**: `[Authorize(Roles = "Housekeeping")]`

**Code Implementation**:
```csharp
[HttpPost("tasks/{id}/claim")]
[Authorize(Roles = "Housekeeping")]
public async Task<IActionResult> ClaimTask(long id)
{
    var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    var task = await _housekeepingService.ClaimTaskAsync(id, userId);
    return Ok(new ApiResponse<HousekeepingTaskDto> { Success = true, Data = task });
}
```

---

### ✅ Task Completion Workflow

**Endpoint**: `PUT /api/housekeeping/tasks/{id}/status`

**HousekeepingService.UpdateTaskStatusAsync() must**:
1. Verify task is assigned to current user (or user is Manager)
2. Update task status (InProgress → Completed)
3. If status is "Completed":
   - Update room status to "Available"
   - Set CompletedAt timestamp
4. Return updated task

---

### 🔐 Role Hierarchy

```
Admin (Full Access)
  ↓
Manager (All operations + reports)
  ↓
Receptionist (Bookings + Guests + Payments)
  ↓
Housekeeping (Own tasks only)
  ↓
Customer (Own bookings only)
```

**Permission Inheritance**:
- Admin can do everything
- Manager can do everything except delete users/system config
- Receptionist can manage bookings, guests, payments
- Housekeeping can only manage own tasks
- Customer can only view/create own bookings

---

## Technical Requirements

### Authorization Attributes
```csharp
[Authorize] // All authenticated
[Authorize(Roles = "Manager")] // Manager only
[Authorize(Roles = "Manager,Receptionist")] // Multiple roles
[AllowAnonymous] // Public endpoint
```

### Common Response Format
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
}
```

### Pagination
```csharp
public class PaginatedResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; }
}
```

---

## Testing Strategy

1. **Unit Tests**: Controller actions with mocked services
2. **Integration Tests**: Full HTTP request/response cycle
3. **Authorization Tests**: Verify role-based access
4. **Validation Tests**: Input validation and error handling

---

## Next Steps

1. Create AuthController first (enables authentication)
2. Implement authorization middleware
3. Create base controller with common methods
4. Implement controllers in priority order
5. Add Swagger documentation
6. Test each controller thoroughly before moving to next

---

**Status**: Ready for implementation  
**Estimated Effort**: 4 weeks (Phase 4A-C), +1 week for Phase 4D
