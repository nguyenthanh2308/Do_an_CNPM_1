// Auth Models
export interface User {
    id: number;
    username: string;
    email: string;
    fullName: string;
    phone?: string;
    role: string; // Admin, Manager, Receptionist, Housekeeping, Customer
    createdAt: Date;
}

export interface LoginRequest {
    username: string;
    password: string;
}

export interface LoginResponse {
    success: boolean;
    message: string;
    data: {
        user: User;
        token: string;
        refreshToken: string;
    };
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    fullName: string;
    phone?: string;
}

export interface RefreshTokenRequest {
    refreshToken: string;
}
