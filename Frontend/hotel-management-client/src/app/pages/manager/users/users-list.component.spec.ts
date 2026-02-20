import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { UsersListComponent } from './users-list.component';
import { ToastService } from '../../../shared/services/toast.service';
import { User } from '../../../models/auth.model';
import { CommonModule } from '@angular/common';

describe('UsersListComponent', () => {
    let component: UsersListComponent;
    let httpMock: HttpTestingController;
    const mockToast = { error: vi.fn(), success: vi.fn(), info: vi.fn() };

    const mockUsers: User[] = [
        { id: 1, username: 'admin', email: 'a@hotel.com', fullName: 'Admin', role: 'Admin', createdAt: new Date() },
        { id: 2, username: 'manager', email: 'm@hotel.com', fullName: 'Manager', role: 'Manager', createdAt: new Date() }
    ];
    const pageResp = { items: mockUsers, totalCount: 2, pageNumber: 1, pageSize: 10, totalPages: 1 };

    beforeEach(async () => {
        vi.clearAllMocks();

        await TestBed.configureTestingModule({
            imports: [UsersListComponent, HttpClientTestingModule, CommonModule, FormsModule],
            providers: [{ provide: ToastService, useValue: mockToast }]
        }).compileComponents();

        const fixture = TestBed.createComponent(UsersListComponent);
        component = fixture.componentInstance;
        httpMock = TestBed.inject(HttpTestingController);

        // Trigger ngOnInit which calls loadUsers() → HTTP GET
        fixture.detectChanges();
        // Flush the initial load request immediately
        httpMock.expectOne(r => r.url.includes('/users')).flush(pageResp);
    });

    afterEach(() => httpMock.verify());

    // ── Creation & initial load ───────────────────────────────
    it('should create', () => expect(component).toBeTruthy());

    it('should load and display 2 users after init', () => {
        expect(component.users().length).toBe(2);
        expect(component.totalItems()).toBe(2);
        expect(component.loading()).toBeFalsy();
    });

    // ── loadUsers error ───────────────────────────────────────
    it('should show error toast on load failure', () => {
        component.loadUsers(1);
        httpMock.expectOne(r => r.url.includes('/users'))
            .flush('err', { status: 500, statusText: 'Error' });
        expect(mockToast.error).toHaveBeenCalledWith('Failed to load users');
        expect(component.loading()).toBeFalsy();
    });

    // ── Pagination ────────────────────────────────────────────
    it('should request correct page and update currentPage', () => {
        component.loadUsers(3);
        const req = httpMock.expectOne(r => r.url.includes('page=3'));
        req.flush({ ...pageResp, pageNumber: 3 });
        expect(component.currentPage()).toBe(3);
    });

    // ── Search / Filter ───────────────────────────────────────
    it('should include searchQuery in URL', () => {
        component.searchQuery = 'admin';
        component.onSearch();
        const req = httpMock.expectOne(r => r.url.includes('search=admin'));
        expect(req.request.url).toContain('search=admin');
        req.flush(pageResp);
    });

    it('should include roleFilter in URL', () => {
        component.roleFilter = 'Manager';
        component.onFilterChange();
        const req = httpMock.expectOne(r => r.url.includes('role=Manager'));
        expect(req.request.url).toContain('role=Manager');
        req.flush(pageResp);
    });

    it('onSearch should reset to page 1', () => {
        const spy = vi.spyOn(component, 'loadUsers');
        component.onSearch();
        expect(spy).toHaveBeenCalledWith(1);
        httpMock.expectOne(r => r.url.includes('/users')).flush(pageResp);
    });

    it('onFilterChange should reset to page 1', () => {
        const spy = vi.spyOn(component, 'loadUsers');
        component.onFilterChange();
        expect(spy).toHaveBeenCalledWith(1);
        httpMock.expectOne(r => r.url.includes('/users')).flush(pageResp);
    });

    // ── deleteUser ────────────────────────────────────────────
    it('should send DELETE and show success toast on confirm', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);

        component.deleteUser(mockUsers[0]);

        // DELETE request
        httpMock.expectOne(r => r.method === 'DELETE' && r.url.endsWith('/users/1'))
            .flush({ success: true, data: true, message: '' });

        // Reload after delete (loadUsers(currentPage))
        httpMock.expectOne(r => r.url.includes('/users')).flush(pageResp);

        expect(mockToast.success).toHaveBeenCalledWith('User "admin" deleted successfully');
    });

    it('should NOT send DELETE when confirm is cancelled', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(false);
        component.deleteUser(mockUsers[0]);
        httpMock.expectNone(r => r.method === 'DELETE');
    });

    it('should show error toast on delete failure', () => {
        vi.spyOn(window, 'confirm').mockReturnValue(true);
        component.deleteUser(mockUsers[0]);
        httpMock.expectOne(r => r.method === 'DELETE')
            .flush('err', { status: 500, statusText: 'Error' });
        expect(mockToast.error).toHaveBeenCalledWith('Failed to delete user');
    });

    // ── getRoleBadgeClass (pure function - no HTTP needed) ────
    const roleCases: [string, string][] = [
        ['Admin', 'role-admin'],
        ['Manager', 'role-manager'],
        ['Receptionist', 'role-receptionist'],
        ['Housekeeping', 'role-housekeeping'],
        ['Customer', 'role-customer'],
        ['Unknown', 'role-default'],
    ];

    roleCases.forEach(([role, cls]) => {
        it(`getRoleBadgeClass('${role}') → '${cls}'`, () => {
            expect(component.getRoleBadgeClass(role)).toBe(cls);
        });
    });
});
