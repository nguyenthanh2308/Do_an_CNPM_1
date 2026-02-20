import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { RoomsListComponent } from './rooms-list.component';
import { RoomApiService } from '../../../services/room-api.service';
import { ToastService } from '../../../shared/services/toast.service';
import { Room } from '../../../models/room.model';
import { PaginatedResponse } from '../../../models/common.model';

describe('RoomsListComponent', () => {
    let component: RoomsListComponent;

    const mockRooms: Room[] = [
        { id: 1, number: '101', roomTypeId: 1, roomTypeName: 'Deluxe', hotelId: 1, hotelName: 'Grand', floor: 1, status: 'Available', basePrice: 100 },
        { id: 2, number: '102', roomTypeId: 1, roomTypeName: 'Deluxe', hotelId: 1, hotelName: 'Grand', floor: 1, status: 'Occupied', basePrice: 150 },
        { id: 3, number: '201', roomTypeId: 2, roomTypeName: 'Suite', hotelId: 1, hotelName: 'Grand', floor: 2, status: 'Maintenance', basePrice: 200 }
    ];
    const pageResp: PaginatedResponse<Room> = {
        items: mockRooms, totalCount: 3, pageNumber: 1, pageSize: 20, totalPages: 1
    };

    const mockRoomApi = {
        getAllRooms: vi.fn().mockReturnValue(of(pageResp)),
        updateRoomStatus: vi.fn(),
        deleteRoom: vi.fn()
    };
    const mockToast = { error: vi.fn(), success: vi.fn(), info: vi.fn() };

    beforeEach(async () => {
        vi.clearAllMocks();
        mockRoomApi.getAllRooms.mockReturnValue(of(pageResp));

        await TestBed.configureTestingModule({
            imports: [RoomsListComponent, FormsModule],
            providers: [
                { provide: RoomApiService, useValue: mockRoomApi },
                { provide: ToastService, useValue: mockToast }
            ]
        }).compileComponents();

        const fixture = TestBed.createComponent(RoomsListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges(); // ngOnInit → loadRooms()
    });

    // ── Creation ──────────────────────────────────────────────
    it('should create', () => expect(component).toBeTruthy());

    it('should populate rooms on init', () => {
        expect(component.rooms().length).toBe(3);
        expect(component.totalItems()).toBe(3);
        expect(component.loading()).toBeFalsy();
    });

    // ── loadRooms ─────────────────────────────────────────────
    it('should pass statusFilter to API', () => {
        component.statusFilter = 'Available';
        component.loadRooms(1);
        expect(mockRoomApi.getAllRooms).toHaveBeenCalledWith(1, 20, 'Available');
    });

    it('should pass undefined when statusFilter is empty', () => {
        component.statusFilter = '';
        component.loadRooms(1);
        expect(mockRoomApi.getAllRooms).toHaveBeenCalledWith(1, 20, undefined);
    });

    it('should show error toast on load failure', () => {
        mockRoomApi.getAllRooms.mockReturnValue(throwError(() => new Error('err')));
        component.loadRooms(1);
        expect(mockToast.error).toHaveBeenCalledWith('Failed to load rooms');
        expect(component.loading()).toBeFalsy();
    });

    it('onFilterChange should reset to page 1', () => {
        const spy = vi.spyOn(component, 'loadRooms');
        component.onFilterChange();
        expect(spy).toHaveBeenCalledWith(1);
    });

    // ── changeRoomStatus ──────────────────────────────────────
    it('should update status after confirm', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        mockRoomApi.updateRoomStatus.mockReturnValue(of({ success: true, data: mockRooms[1], message: '' }));
        component.changeRoomStatus(mockRooms[1], 'Available');
        expect(mockRoomApi.updateRoomStatus).toHaveBeenCalledWith(2, 'Available');
        expect(mockToast.success).toHaveBeenCalled();
    });

    it('should NOT update status if confirm is cancelled', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(false);
        component.changeRoomStatus(mockRooms[0], 'Maintenance');
        expect(mockRoomApi.updateRoomStatus).not.toHaveBeenCalled();
    });

    it('should show error toast on status update failure', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        mockRoomApi.updateRoomStatus.mockReturnValue(throwError(() => new Error('err')));
        component.changeRoomStatus(mockRooms[0], 'Maintenance');
        expect(mockToast.error).toHaveBeenCalledWith('Failed to update room status');
    });

    // ── deleteRoom ────────────────────────────────────────────
    it('should delete room after confirm', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        mockRoomApi.deleteRoom.mockReturnValue(of({ success: true, data: true, message: '' }));
        component.deleteRoom(mockRooms[2]);
        expect(mockRoomApi.deleteRoom).toHaveBeenCalledWith(3);
        expect(mockToast.success).toHaveBeenCalled();
    });

    it('should NOT delete room if confirm is cancelled', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(false);
        component.deleteRoom(mockRooms[0]);
        expect(mockRoomApi.deleteRoom).not.toHaveBeenCalled();
    });

    it('should show error toast on delete failure', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        mockRoomApi.deleteRoom.mockReturnValue(throwError(() => new Error('err')));
        component.deleteRoom(mockRooms[0]);
        expect(mockToast.error).toHaveBeenCalledWith('Failed to delete room');
    });

    // ── getStatusClass / getStatusIcon ────────────────────────
    const statusCases: [string, string][] = [
        ['Available', 'status-available'],
        ['Occupied', 'status-occupied'],
        ['Reserved', 'status-reserved'],
        ['Maintenance', 'status-maintenance'],
        ['Unknown', 'status-default'],
    ];

    statusCases.forEach(([status, cls]) => {
        it(`getStatusClass('${status}') → '${cls}'`, () => {
            expect(component.getStatusClass(status)).toBe(cls);
        });
    });

    it('getStatusIcon Available → ✓', () => {
        expect(component.getStatusIcon('Available')).toBe('✓');
    });

    it('getStatusIcon Occupied → ●', () => {
        expect(component.getStatusIcon('Occupied')).toBe('●');
    });

    it('getStatusIcon Unknown → ○', () => {
        expect(component.getStatusIcon('Unknown')).toBe('○');
    });
});
