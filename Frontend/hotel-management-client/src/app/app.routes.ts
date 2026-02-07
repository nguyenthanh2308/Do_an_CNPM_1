import { Routes } from '@angular/router';
import { authGuard } from './auth/guards/auth.guard';
import { roleGuard } from './auth/guards/role.guard';

export const routes: Routes = [
    // Default redirect
    { path: '', redirectTo: '/login', pathMatch: 'full' },

    // Public routes
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
    },
    {
        path: 'register',
        loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
    },
    {
        path: 'unauthorized',
        loadComponent: () => import('./pages/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
    },

    // Manager routes
    {
        path: 'manager',
        canActivate: [authGuard, roleGuard(['Manager', 'Admin'])],
        loadComponent: () => import('./layouts/manager-layout/manager-layout.component').then(m => m.ManagerLayoutComponent),
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            {
                path: 'dashboard',
                loadComponent: () => import('./pages/manager/dashboard/dashboard.component').then(m => m.DashboardComponent)
            },
            {
                path: 'users',
                loadComponent: () => import('./pages/manager/users/users-list.component').then(m => m.UsersListComponent)
            },
            {
                path: 'rooms',
                loadComponent: () => import('./pages/manager/rooms/rooms-list.component').then(m => m.RoomsListComponent)
            },
            {
                path: 'room-types',
                loadComponent: () => import('./pages/manager/room-types/room-types-list.component').then(m => m.RoomTypesListComponent)
            },
            {
                path: 'reports',
                loadComponent: () => import('./pages/manager/reports/reports.component').then(m => m.ReportsComponent)
            }
        ]
    },

    // Future: Receptionist routes
    // {
    //   path: 'receptionist',
    //   canActivate: [authGuard, roleGuard(['Receptionist', 'Manager', 'Admin'])],
    //   loadComponent: () => import('./layouts/receptionist-layout.component').then(m => m.ReceptionistLayoutComponent),
    //   children: [...]
    // },

    // Future: Housekeeping routes
    // {
    //   path: 'housekeeping',
    //   canActivate: [authGuard, roleGuard(['Housekeeping'])],
    //   loadComponent: () => import('./layouts/housekeeping-layout.component').then(m => m.HousekeepingLayoutComponent),
    //   children: [...]
    // },

    // Future: Customer routes
    // {
    //   path: 'customer',
    //   canActivate: [authGuard, roleGuard(['Customer'])],
    //   loadComponent: () => import('./layouts/customer-layout.component').then(m => m.CustomerLayoutComponent),
    //   children: [...]
    // },

    // Wildcard
    { path: '**', redirectTo: '/login' }
];
