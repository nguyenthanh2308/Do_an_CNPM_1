import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { User, LoginRequest, LoginResponse, RegisterRequest } from '../../models/auth.model';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly TOKEN_KEY = 'auth_token';
    private readonly REFRESH_TOKEN_KEY = 'refresh_token';
    private readonly USER_KEY = 'current_user';

    private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
    public currentUser$ = this.currentUserSubject.asObservable();

    // Signal for better reactivity in Angular 21
    public currentUserSignal = signal<User | null>(this.getUserFromStorage());

    constructor(
        private http: HttpClient,
        private router: Router
    ) { }

    /**
     * Login user with credentials
     */
    login(credentials: LoginRequest): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, credentials)
            .pipe(
                tap(response => {
                    if (response.success && response.data) {
                        this.setSession(response.data.user, response.data.token, response.data.refreshToken);
                    }
                }),
                catchError(error => {
                    console.error('Login error:', error);
                    return throwError(() => error);
                })
            );
    }

    /**
     * Register new user
     */
    register(data: RegisterRequest): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/register`, data)
            .pipe(
                tap(response => {
                    if (response.success && response.data) {
                        this.setSession(response.data.user, response.data.token, response.data.refreshToken);
                    }
                })
            );
    }

    /**
     * Logout current user
     */
    logout(): void {
        // Clear storage
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.REFRESH_TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);

        // Update observables
        this.currentUserSubject.next(null);
        this.currentUserSignal.set(null);

        // Redirect to login
        this.router.navigate(['/login']);
    }

    /**
     * Refresh access token
     */
    refreshToken(): Observable<LoginResponse> {
        const refreshToken = this.getRefreshToken();
        if (!refreshToken) {
            return throwError(() => new Error('No refresh token available'));
        }

        return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/refresh-token`, {
            refreshToken
        }).pipe(
            tap(response => {
                if (response.success && response.data) {
                    this.setSession(response.data.user, response.data.token, response.data.refreshToken);
                }
            }),
            catchError(error => {
                this.logout();
                return throwError(() => error);
            })
        );
    }

    /**
     * Get current user
     */
    getCurrentUser(): User | null {
        return this.currentUserSubject.value;
    }

    /**
     * Check if user is authenticated
     */
    isAuthenticated(): boolean {
        const token = this.getToken();
        if (!token) return false;

        // Check if token is expired
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const expiry = payload.exp * 1000; // Convert to milliseconds
            return Date.now() < expiry;
        } catch {
            return false;
        }
    }

    /**
     * Check if user has specific role
     */
    hasRole(role: string | string[]): boolean {
        const user = this.getCurrentUser();
        if (!user) return false;

        if (Array.isArray(role)) {
            return role.includes(user.role);
        }
        return user.role === role;
    }

    /**
     * Get JWT token
     */
    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    /**
     * Get refresh token
     */
    getRefreshToken(): string | null {
        return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }

    /**
     * Set session data
     */
    private setSession(user: User, token: string, refreshToken: string): void {
        localStorage.setItem(this.TOKEN_KEY, token);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));

        this.currentUserSubject.next(user);
        this.currentUserSignal.set(user);
    }

    /**
     * Get user from localStorage
     */
    private getUserFromStorage(): User | null {
        const userJson = localStorage.getItem(this.USER_KEY);
        if (!userJson) return null;

        try {
            return JSON.parse(userJson);
        } catch {
            return null;
        }
    }
}
