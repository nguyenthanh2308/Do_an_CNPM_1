import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { RoomTypesListComponent } from './room-types-list.component';
import { RoomApiService } from '../../../services/room-api.service';
import { ToastService } from '../../../shared/services/toast.service';
import { RoomType, Amenity } from '../../../models/room.model';
import { ApiResponse } from '../../../models/common.model';

describe('RoomTypesListComponent', () => {
    let component: RoomTypesListComponent;

    const amenities: Amenity[] = [
        { id: 1, name: 'WiFi', description: 'Free WiFi' },
        { id: 2, name: 'Mini Bar', description: 'Stocked mini bar' }
    ];

    const mockRoomTypes: RoomType[] = [
        { id: 1, name: 'Deluxe', description: 'Comfortable room', basePrice: 150, maxOccupancy: 2, bedType: 'King', size: 30, amenities },
        { id: 2, name: 'Suite', description: 'Luxury suite', basePrice: 350, maxOccupancy: 4, bedType: 'Twin', size: 60, amenities: [] }
    ];

    const apiResp: ApiResponse<RoomType[]> = { success: true, data: mockRoomTypes, message: 'OK' };

    const mockRoomApi = { getAllRoomTypes: vi.fn().mockReturnValue(of(apiResp)) };
    const mockToast = { error: vi.fn(), success: vi.fn(), info: vi.fn() };

    beforeEach(async () => {
        vi.clearAllMocks();
        mockRoomApi.getAllRoomTypes.mockReturnValue(of(apiResp));

        await TestBed.configureTestingModule({
            imports: [RoomTypesListComponent],
            providers: [
                { provide: RoomApiService, useValue: mockRoomApi },
                { provide: ToastService, useValue: mockToast }
            ]
        }).compileComponents();

        const fixture = TestBed.createComponent(RoomTypesListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges(); // triggers ngOnInit
    });

    // ── Creation ──────────────────────────────────────────────
    it('should create', () => expect(component).toBeTruthy());

    it('should load room types on init', () => {
        expect(component.roomTypes().length).toBe(2);
    });

    it('should set loading=false after load', () => {
        expect(component.loading()).toBeFalsy();
    });

    // ── Error handling ────────────────────────────────────────
    it('should show error toast on API failure', () => {
        mockRoomApi.getAllRoomTypes.mockReturnValue(throwError(() => new Error('err')));
        component.loadRoomTypes();
        expect(mockToast.error).toHaveBeenCalledWith('Failed to load room types');
        expect(component.loading()).toBeFalsy();
    });

    it('should handle empty/null response gracefully', () => {
        mockRoomApi.getAllRoomTypes.mockReturnValue(of({ success: true, data: null as any, message: '' }));
        expect(() => component.loadRoomTypes()).not.toThrow();
    });

    // ── deleteRoomType (placeholder) ──────────────────────────
    it('deleteRoomType should show info toast', () => {
        component.deleteRoomType(mockRoomTypes[0]);
        expect(mockToast.info).toHaveBeenCalledWith('Delete room type feature coming soon');
    });

    // ── Data integrity ────────────────────────────────────────
    it('first room type name should be Deluxe', () => {
        expect(component.roomTypes()[0].name).toBe('Deluxe');
    });

    it('first room type should have 2 amenities', () => {
        const rt = component.roomTypes()[0];
        expect(rt.amenities.length).toBe(2);
        expect(rt.amenities[0].name).toBe('WiFi');
    });

    it('second room type should have empty amenities', () => {
        expect(component.roomTypes()[1].amenities.length).toBe(0);
    });

    it('room types prices should be correct', () => {
        expect(component.roomTypes()[0].basePrice).toBe(150);
        expect(component.roomTypes()[1].basePrice).toBe(350);
    });
});
