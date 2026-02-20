import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { ReportsComponent } from './reports.component';
import { ReportApiService, RevenueReport, OccupancyReport } from '../../../services/report-api.service';
import { ToastService } from '../../../shared/services/toast.service';

describe('ReportsComponent', () => {
    let component: ReportsComponent;

    const mockRevenue: RevenueReport = {
        period: '2026-02',
        totalRevenue: 15000,
        totalBookings: 30,
        averageBookingValue: 500,
        paymentMethodBreakdown: []
    };

    const mockOccupancy: OccupancyReport = {
        date: new Date(),
        totalRooms: 50,
        occupiedRooms: 35,
        occupancyRate: 70,
        revenue: 5000
    };

    const mockReportApi = {
        getRevenueReport: vi.fn(),
        getOccupancyReport: vi.fn(),
        exportToCsv: vi.fn()
    };
    const mockToast = { error: vi.fn(), success: vi.fn(), info: vi.fn() };

    beforeEach(async () => {
        vi.clearAllMocks();
        mockReportApi.getRevenueReport.mockReturnValue(of({ success: true, data: mockRevenue, message: '' }));
        mockReportApi.getOccupancyReport.mockReturnValue(of({ success: true, data: [mockOccupancy], message: '' }));

        await TestBed.configureTestingModule({
            imports: [ReportsComponent, FormsModule],
            providers: [
                { provide: ReportApiService, useValue: mockReportApi },
                { provide: ToastService, useValue: mockToast }
            ]
        }).compileComponents();

        const fixture = TestBed.createComponent(ReportsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges(); // triggers ngOnInit
    });

    // ── Creation ──────────────────────────────────────────────
    it('should create', () => expect(component).toBeTruthy());

    // ── Default date range ────────────────────────────────────
    it('should default endDate to today', () => {
        const today = new Date().toISOString().split('T')[0];
        expect(component.dateRange().endDate).toBe(today);
    });

    it('startDate should be approximately 30 days ago', () => {
        const start = new Date(component.dateRange().startDate);
        const diffDays = Math.round((Date.now() - start.getTime()) / 86_400_000);
        expect(diffDays).toBeGreaterThanOrEqual(29);
        expect(diffDays).toBeLessThanOrEqual(31);
    });

    // ── API calls on init ─────────────────────────────────────
    it('should call getRevenueReport on init', async () => {
        await vi.waitFor(() => expect(mockReportApi.getRevenueReport).toHaveBeenCalled());
    });

    it('should call getOccupancyReport on init', async () => {
        await vi.waitFor(() => expect(mockReportApi.getOccupancyReport).toHaveBeenCalled());
    });

    it('should pass Date objects to getRevenueReport', async () => {
        await vi.waitFor(() => {
            const [start, end] = mockReportApi.getRevenueReport.mock.calls[0];
            expect(start).toBeInstanceOf(Date);
            expect(end).toBeInstanceOf(Date);
        });
    });

    it('should populate revenueData', async () => {
        component.loadReports();
        await vi.waitFor(() => expect(component.revenueData()).toEqual(mockRevenue));
    });

    it('should populate occupancyData', async () => {
        component.loadReports();
        await vi.waitFor(() => expect(component.occupancyData()).toEqual([mockOccupancy]));
    });

    it('should set loading=false after successful load', async () => {
        component.loadReports();
        await vi.waitFor(() => expect(component.loading()).toBeFalsy());
    });

    // ── Error handling ────────────────────────────────────────
    it('should show error when revenue report fails', async () => {
        mockReportApi.getRevenueReport.mockReturnValue(throwError(() => new Error('err')));
        component.loadReports();
        await vi.waitFor(() =>
            expect(mockToast.error).toHaveBeenCalledWith('Failed to load revenue report')
        );
    });

    it('should show error when occupancy report fails', async () => {
        mockReportApi.getOccupancyReport.mockReturnValue(throwError(() => new Error('err')));
        component.loadReports();
        await vi.waitFor(() =>
            expect(mockToast.error).toHaveBeenCalledWith('Failed to load occupancy report')
        );
    });

    // ── User actions ──────────────────────────────────────────
    it('onDateRangeChange should call loadReports', () => {
        const spy = vi.spyOn(component, 'loadReports');
        component.onDateRangeChange();
        expect(spy).toHaveBeenCalled();
    });

    it('exportRevenueCsv should show info toast', () => {
        component.exportRevenueCsv();
        expect(mockToast.info).toHaveBeenCalledWith('CSV export feature coming soon');
    });
});
