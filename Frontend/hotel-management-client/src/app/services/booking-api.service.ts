import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import {
    Booking,
    BookingDetail,
    CreateBooking,
    UpdateBooking
} from '../models/booking.model';
import { ApiResponse, PaginatedResponse } from '../models/common.model';

@Injectable({
    providedIn: 'root'
})
export class BookingApiService {
    private readonly apiUrl = `${environment.apiUrl}/bookings`;

    constructor(private http: HttpClient) { }

    /**
     * Get all bookings with pagination
     */
    getAll(page = 1, pageSize = 10, status?: string): Observable<PaginatedResponse<Booking>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (status) {
            params = params.set('status', status);
        }

        return this.http.get<PaginatedResponse<Booking>>(this.apiUrl, { params });
    }

    /**
     * Get booking by ID with full details
     */
    getById(id: number): Observable<ApiResponse<BookingDetail>> {
        return this.http.get<ApiResponse<BookingDetail>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create new booking
     */
    create(booking: CreateBooking): Observable<ApiResponse<Booking>> {
        return this.http.post<ApiResponse<Booking>>(this.apiUrl, booking);
    }

    /**
     * Update existing booking
     */
    update(id: number, booking: UpdateBooking): Observable<ApiResponse<Booking>> {
        return this.http.put<ApiResponse<Booking>>(`${this.apiUrl}/${id}`, booking);
    }

    /**
     * Check-in booking
     */
    checkIn(id: number): Observable<ApiResponse<Booking>> {
        return this.http.post<ApiResponse<Booking>>(`${this.apiUrl}/${id}/checkin`, {});
    }

    /**
     * Check-out booking
     */
    checkOut(id: number): Observable<ApiResponse<Booking>> {
        return this.http.post<ApiResponse<Booking>>(`${this.apiUrl}/${id}/checkout`, {});
    }

    /**
     * Cancel booking
     */
    cancel(id: number): Observable<ApiResponse<boolean>> {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Get user's bookings (for customer portal)
     */
    getMyBookings(page = 1, pageSize = 10): Observable<PaginatedResponse<Booking>> {
        const params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        return this.http.get<PaginatedResponse<Booking>>(`${this.apiUrl}/my-bookings`, { params });
    }
}
