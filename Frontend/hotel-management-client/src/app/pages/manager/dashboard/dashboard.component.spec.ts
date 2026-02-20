import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { DashboardComponent } from './dashboard.component';
import { BookingApiService } from '../../../services/booking-api.service';
import { RoomApiService } from '../../../services/room-api.service';
import { ReportApiService } from '../../../services/report-api.service';
import { ToastService } from '../../../shared/services/toast.service';
import { PaginatedResponse } from '../../../models/common.model';
import { Booking } from '../../../models/booking.model';
import { Room } from '../../../models/room.model';

describe('DashboardComponent', () => {
    let component: DashboardComponent;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const mockBookings: Booking[] = [
        {
            id: 1, guestId: 1, guestName: 'John Doe',
            hotelId: 1, hotelName: 'Grand Hotel',
            checkInDate: today,
            checkOutDate: new Date('2026-02-10'),
            status: 'Confirmed', paymentStatus: 'Paid',
            totalAmount: 500, createdAt: new Date()
        },
        {
            id: 2, guestId: 2, guestName: 'Jane Smith',
            hotelId: 1, hotelName: 'Grand Hotel',
            checkInDate: new Date('2026-02-15'),
            checkOutDate: new Date('2026-02-20'),
            status: 'Pending', paymentStatus: 'Unpaid',
            totalAmount: 750, createdAt: new Date()
        }
    ];

    const mockBookingResponse: PaginatedResponse<Booking> = {
        items: mockBookings, totalCount: 2, pageNumber: 1, pageSize: 100, totalPages: 1
    };
    const mockRoomResponse: PaginatedResponse<Room> = {
        items: [], totalCount: 3, pageNumber: 1, pageSize: 100, totalPages: 1
    };

    const mockBookingApi = { getAll: vi.fn() };
    const mockRoomApi = { getAllRooms: vi.fn() };
    const mockReportApi = {};
    const mockToast = { error: vi.fn(), success: vi.fn(), info: vi.fn() };

    beforeEach(async () => {
        vi.clearAllMocks();
        mockBookingApi.getAll.mockReturnValue(of(mockBookingResponse));
        mockRoomApi.getAllRooms.mockReturnValue(of(mockRoomResponse));

        await TestBed.configureTestingModule({
            imports: [DashboardComponent],
            providers: [
                { provide: BookingApiService, useValue: mockBookingApi },
                { provide: RoomApiService, useValue: mockRoomApi },
                { provide: ReportApiService, useValue: mockReportApi },
                { provide: ToastService, useValue: mockToast }
            ]
        }).compileComponents();

        const fixture = TestBed.createComponent(DashboardComponent);
        component = fixture.componentInstance;
    });

    // ── Synchronous: initial state ────────────────────────────
    it('should create', () => expect(component).toBeTruthy());

    it('should initialise stats to zero', () => {
        const s = component.stats();
        expect(s.totalRevenue).toBe(0);
        expect(s.totalBookings).toBe(0);
        expect(s.todayCheckIns).toBe(0);
        expect(s.availableRooms).toBe(0);
    });

    it('should expose currentDate as a Date instance', () => {
        expect(component.currentDate).toBeInstanceOf(Date);
    });

    // ── Async: after data loads ───────────────────────────────
    it('should call bookingApi.getAll on init', async () => {
        component.ngOnInit();
        await vi.waitFor(() => expect(mockBookingApi.getAll).toHaveBeenCalled());
    });

    it('should call roomApi.getAllRooms on init', async () => {
        component.ngOnInit();
        await vi.waitFor(() => expect(mockRoomApi.getAllRooms).toHaveBeenCalled());
    });

    it('should calculate total revenue = 1250', async () => {
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.stats().totalRevenue).toBe(1250));
    });

    it('should set totalBookings = 2', async () => {
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.stats().totalBookings).toBe(2));
    });

    it('should count 1 today check-in', async () => {
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.stats().todayCheckIns).toBe(1));
    });

    it('should set availableRooms = 3 (from totalCount)', async () => {
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.stats().availableRooms).toBe(3));
    });

    it('should populate recentBookings', async () => {
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.recentBookings().length).toBe(2));
    });

    it('should set loading=false after data loads', async () => {
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.loading()).toBeFalsy());
    });

    it('loading returns to false even when API calls throw errors', async () => {
        // Component catches errors inside each private method's try-catch,
        // so Promise.all never rejects and loading always resets properly
        mockBookingApi.getAll.mockReturnValue(throwError(() => new Error('err')));
        mockRoomApi.getAllRooms.mockReturnValue(throwError(() => new Error('err')));
        component.loadDashboardData();
        await vi.waitFor(() => expect(component.loading()).toBeFalsy());
        // Stats remain at 0 because data was never loaded
        expect(component.stats().totalRevenue).toBe(0);
        expect(component.stats().availableRooms).toBe(0);
    });


    // ── refresh() ─────────────────────────────────────────────
    it('refresh() should show info toast', () => {
        component.refresh();
        expect(mockToast.info).toHaveBeenCalledWith('Dashboard refreshed');
    });

    // ── getStatusClass() ──────────────────────────────────────
    const statusCases: [string, string][] = [
        ['Pending', 'status-pending'],
        ['Confirmed', 'status-confirmed'],
        ['CheckedIn', 'status-checkedin'],
        ['CheckedOut', 'status-checkedout'],
        ['Cancelled', 'status-cancelled'],
        ['Unknown', 'status-default'],
    ];

    statusCases.forEach(([status, expected]) => {
        it(`getStatusClass('${status}') → '${expected}'`, () => {
            expect(component.getStatusClass(status)).toBe(expected);
        });
    });
});
