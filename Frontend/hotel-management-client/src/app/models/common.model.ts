// Common API Response Models
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    errors?: string[];
}

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages?: number;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

// Housekeeping Models
export interface HousekeepingTask {
    id: number;
    roomId: number;
    roomNumber: string;
    taskType: string;
    status: string; // Pending, InProgress, Completed, Cancelled
    priority: string; // Low, Normal, High
    assignedToUserId?: number;
    assignedToUserName?: string;
    notes?: string;
    createdAt: Date;
    completedAt?: Date;
}

export interface CreateHousekeepingTask {
    roomId: number;
    taskType: string;
    priority?: string;
    notes?: string;
}
