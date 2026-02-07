import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { ApiResponse } from '../models/common.model';

// Report interfaces
export interface OccupancyReport {
    date: Date;
    totalRooms: number;
    occupiedRooms: number;
    occupancyRate: number;
    revenue: number;
}

export interface RevenueReport {
    period: string;
    totalRevenue: number;
    totalBookings: number;
    averageBookingValue: number;
    paymentMethodBreakdown: { method: string; amount: number }[];
}

export interface BookingStatusReport {
    status: string;
    count: number;
    percentage: number;
}

@Injectable({
    providedIn: 'root'
})
export class ReportApiService {
    private readonly apiUrl = `${environment.apiUrl}/reports`;

    constructor(private http: HttpClient) { }

    /**
     * Get occupancy report
     */
    getOccupancyReport(
        startDate: Date,
        endDate: Date,
        hotelId?: number
    ): Observable<ApiResponse<OccupancyReport[]>> {
        let params = new HttpParams()
            .set('startDate', startDate.toISOString())
            .set('endDate', endDate.toISOString());

        if (hotelId) params = params.set('hotelId', hotelId.toString());

        return this.http.get<ApiResponse<OccupancyReport[]>>(`${this.apiUrl}/occupancy`, { params });
    }

    /**
     * Get revenue report
     */
    getRevenueReport(
        startDate: Date,
        endDate: Date,
        hotelId?: number
    ): Observable<ApiResponse<RevenueReport>> {
        let params = new HttpParams()
            .set('startDate', startDate.toISOString())
            .set('endDate', endDate.toISOString());

        if (hotelId) params = params.set('hotelId', hotelId.toString());

        return this.http.get<ApiResponse<RevenueReport>>(`${this.apiUrl}/revenue`, { params });
    }

    /**
     * Get booking status distribution
     */
    getBookingStatusReport(
        startDate: Date,
        endDate: Date
    ): Observable<ApiResponse<BookingStatusReport[]>> {
        const params = new HttpParams()
            .set('startDate', startDate.toISOString())
            .set('endDate', endDate.toISOString());

        return this.http.get<ApiResponse<BookingStatusReport[]>>(`${this.apiUrl}/booking-status`, {
            params
        });
    }

    /**
     * Export report to CSV
     */
    exportToCsv(reportType: string, startDate: Date, endDate: Date): Observable<Blob> {
        const params = new HttpParams()
            .set('type', reportType)
            .set('startDate', startDate.toISOString())
            .set('endDate', endDate.toISOString());

        return this.http.get(`${this.apiUrl}/export`, {
            responseType: 'blob',
            params
        });
    }
}
