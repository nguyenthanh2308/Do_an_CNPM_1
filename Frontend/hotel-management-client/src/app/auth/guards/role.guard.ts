import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Role Guard Factory - Creates a guard that checks for specific roles
 * Usage: canActivate: [authGuard, roleGuard(['Manager', 'Admin'])]
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
    return (route, state) => {
        const authService = inject(AuthService);
        const router = inject(Router);

        const user = authService.getCurrentUser();

        // Check if user exists and has allowed role
        if (user && allowedRoles.includes(user.role)) {
            return true;
        }

        // Redirect to unauthorized page or home
        router.navigate(['/unauthorized']);
        return false;
    };
};

/**
 * Simple role guard for single role
 */
export const singleRoleGuard = (role: string): CanActivateFn => {
    return roleGuard([role]);
};
