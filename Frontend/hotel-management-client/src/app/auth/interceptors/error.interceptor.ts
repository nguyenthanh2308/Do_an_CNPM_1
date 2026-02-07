import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Error Interceptor - Handles HTTP errors globally
 * - 401: Unauthorized -> logout and redirect to login
 * - 403: Forbidden -> redirect to unauthorized page
 * - Other errors: pass through for component handling
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
                // Unauthorized - token expired or invalid
                console.error('Unauthorized access - logging out');
                authService.logout();
                router.navigate(['/login']);
            } else if (error.status === 403) {
                // Forbidden - user doesn't have permission
                console.error('Forbidden access');
                router.navigate(['/unauthorized']);
            } else if (error.status === 0) {
                // Network error - backend might be down
                console.error('Network error - cannot reach server');
            }

            // Pass error to calling component for specific handling
            return throwError(() => error);
        })
    );
};
