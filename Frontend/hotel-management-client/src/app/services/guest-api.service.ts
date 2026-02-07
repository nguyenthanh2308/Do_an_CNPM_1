import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Guest, CreateGuest, UpdateGuest } from '../models/guest.model';
import { ApiResponse, PaginatedResponse } from '../models/common.model';

@Injectable({
    providedIn: 'root'
})
export class GuestApiService {
    private readonly apiUrl = `${environment.apiUrl}/guests`;

    constructor(private http: HttpClient) { }

    /**
     * Get all guests with pagination
     */
    getAll(page = 1, pageSize = 10, search?: string): Observable<PaginatedResponse<Guest>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (search) {
            params = params.set('search', search);
        }

        return this.http.get<PaginatedResponse<Guest>>(this.apiUrl, { params });
    }

    /**
     * Get guest by ID
     */
    getById(id: number): Observable<ApiResponse<Guest>> {
        return this.http.get<ApiResponse<Guest>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create new guest
     */
    create(guest: CreateGuest): Observable<ApiResponse<Guest>> {
        return this.http.post<ApiResponse<Guest>>(this.apiUrl, guest);
    }

    /**
     * Update guest
     */
    update(id: number, guest: UpdateGuest): Observable<ApiResponse<Guest>> {
        return this.http.put<ApiResponse<Guest>>(`${this.apiUrl}/${id}`, guest);
    }

    /**
     * Delete guest
     */
    delete(id: number): Observable<ApiResponse<boolean>> {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Search guests by email or phone
     */
    search(query: string): Observable<ApiResponse<Guest[]>> {
        const params = new HttpParams().set('q', query);
        return this.http.get<ApiResponse<Guest[]>>(`${this.apiUrl}/search`, { params });
    }
}
