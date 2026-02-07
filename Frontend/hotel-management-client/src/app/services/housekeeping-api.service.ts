import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { HousekeepingTask, CreateHousekeepingTask } from '../models/common.model';
import { ApiResponse, PaginatedResponse } from '../models/common.model';

@Injectable({
    providedIn: 'root'
})
export class HousekeepingApiService {
    private readonly apiUrl = `${environment.apiUrl}/housekeeping`;

    constructor(private http: HttpClient) { }

    /**
     * Get all housekeeping tasks with pagination
     */
    getAllTasks(
        page = 1,
        pageSize = 10,
        status?: string,
        priority?: string
    ): Observable<PaginatedResponse<HousekeepingTask>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (status) params = params.set('status', status);
        if (priority) params = params.set('priority', priority);

        return this.http.get<PaginatedResponse<HousekeepingTask>>(this.apiUrl, { params });
    }

    /**
     * Get task by ID
     */
    getTaskById(id: number): Observable<ApiResponse<HousekeepingTask>> {
        return this.http.get<ApiResponse<HousekeepingTask>>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create housekeeping task
     */
    createTask(task: CreateHousekeepingTask): Observable<ApiResponse<HousekeepingTask>> {
        return this.http.post<ApiResponse<HousekeepingTask>>(this.apiUrl, task);
    }

    /**
     * Update task status
     */
    updateTaskStatus(id: number, status: string): Observable<ApiResponse<HousekeepingTask>> {
        return this.http.patch<ApiResponse<HousekeepingTask>>(`${this.apiUrl}/${id}/status`, {
            status
        });
    }

    /**
     * Assign task to user
     */
    assignTask(id: number, userId: number): Observable<ApiResponse<HousekeepingTask>> {
        return this.http.patch<ApiResponse<HousekeepingTask>>(`${this.apiUrl}/${id}/assign`, {
            userId
        });
    }

    /**
     * Claim task (housekeeping staff self-assigns)
     */
    claimTask(id: number): Observable<ApiResponse<HousekeepingTask>> {
        return this.http.post<ApiResponse<HousekeepingTask>>(`${this.apiUrl}/${id}/claim`, {});
    }

    /**
     * Add notes to task
     */
    addNotes(id: number, notes: string): Observable<ApiResponse<HousekeepingTask>> {
        return this.http.patch<ApiResponse<HousekeepingTask>>(`${this.apiUrl}/${id}/notes`, {
            notes
        });
    }

    /**
     * Get my assigned tasks (for housekeeping staff)
     */
    getMyTasks(page = 1, pageSize = 10): Observable<PaginatedResponse<HousekeepingTask>> {
        const params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        return this.http.get<PaginatedResponse<HousekeepingTask>>(`${this.apiUrl}/my-tasks`, {
            params
        });
    }
}
