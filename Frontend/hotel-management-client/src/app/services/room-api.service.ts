import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import {
    Room,
    RoomType,
    CreateRoom,
    UpdateRoom,
    RoomAvailability
} from '../models/room.model';
import { ApiResponse, PaginatedResponse } from '../models/common.model';

@Injectable({
    providedIn: 'root'
})
export class RoomApiService {
    private readonly apiUrl = `${environment.apiUrl}/rooms`;
    private readonly roomTypeUrl = `${environment.apiUrl}/roomtypes`;

    constructor(private http: HttpClient) { }

    // ========== Room Operations ==========

    /**
     * Get all rooms with pagination
     */
    getAllRooms(page = 1, pageSize = 10, status?: string): Observable<PaginatedResponse<Room>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (status) {
            params = params.set('status', status);
        }

        return this.http.get<PaginatedResponse<Room>>(this.apiUrl, { params });
    }

    /**
     * Get room by ID
     */
    getRoomById(id: number): Observable<ApiResponse<Room>> {
        return this.http.get<ApiResponse<Room>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create new room
     */
    createRoom(room: CreateRoom): Observable<ApiResponse<Room>> {
        return this.http.post<ApiResponse<Room>>(this.apiUrl, room);
    }

    /**
     * Update room
     */
    updateRoom(id: number, room: UpdateRoom): Observable<ApiResponse<Room>> {
        return this.http.put<ApiResponse<Room>>(`${this.apiUrl}/${id}`, room);
    }

    /**
     * Delete room
     */
    deleteRoom(id: number): Observable<ApiResponse<boolean>> {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Update room status
     */
    updateRoomStatus(id: number, status: string): Observable<ApiResponse<Room>> {
        return this.http.patch<ApiResponse<Room>>(`${this.apiUrl}/${id}/status`, { status });
    }

    /**
     * Check room availability
     */
    checkAvailability(
        checkIn: Date,
        checkOut: Date,
        roomTypeId?: number
    ): Observable<ApiResponse<RoomAvailability[]>> {
        let params = new HttpParams()
            .set('checkIn', checkIn.toISOString())
            .set('checkOut', checkOut.toISOString());

        if (roomTypeId) {
            params = params.set('roomTypeId', roomTypeId.toString());
        }

        return this.http.get<ApiResponse<RoomAvailability[]>>(`${this.apiUrl}/availability`, {
            params
        });
    }

    // ========== Room Type Operations ==========

    /**
     * Get all room types
     */
    getAllRoomTypes(): Observable<ApiResponse<RoomType[]>> {
        return this.http.get<ApiResponse<RoomType[]>>(this.roomTypeUrl);
    }

    /**
     * Get room type by ID
     */
    getRoomTypeById(id: number): Observable<ApiResponse<RoomType>> {
        return this.http.get<ApiResponse<RoomType>>(`${this.roomTypeUrl}/${id}`);
    }
}
