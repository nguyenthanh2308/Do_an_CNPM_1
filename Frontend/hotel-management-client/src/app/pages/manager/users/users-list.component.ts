import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';
import { User } from '../../../models/auth.model';
import { ApiResponse, PaginatedResponse } from '../../../models/common.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent, PaginationComponent],
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.css']
})
export class UsersListComponent implements OnInit {
  users = signal<User[]>([]);
  loading = signal(false);

  // Pagination
  currentPage = signal(1);
  pageSize = 10;
  totalPages = signal(1);
  totalItems = signal(0);

  // Filters
  searchQuery = '';
  roleFilter = '';

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers(page = 1) {
    this.loading.set(true);
    this.currentPage.set(page);

    let url = `${environment.apiUrl}/users?page=${page}&pageSize=${this.pageSize}`;

    if (this.searchQuery) {
      url += `&search=${encodeURIComponent(this.searchQuery)}`;
    }

    if (this.roleFilter) {
      url += `&role=${this.roleFilter}`;
    }

    this.http.get<PaginatedResponse<User>>(url).subscribe({
      next: (response) => {
        this.users.set(response.items);
        this.totalPages.set(response.totalPages || Math.ceil(response.totalCount / this.pageSize));
        this.totalItems.set(response.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error('Failed to load users');
        console.error('Error loading users:', err);
      }
    });
  }

  onSearch() {
    this.loadUsers(1); // Reset to first page on search
  }

  onFilterChange() {
    this.loadUsers(1); // Reset to first page on filter change
  }

  deleteUser(user: User) {
    if (confirm(`Are you sure you want to delete user "${user.username}"?`)) {
      this.http.delete<ApiResponse<boolean>>(`${environment.apiUrl}/users/${user.id}`).subscribe({
        next: () => {
          this.toastService.success(`User "${user.username}" deleted successfully`);
          this.loadUsers(this.currentPage()); // Reload current page
        },
        error: (err) => {
          this.toastService.error('Failed to delete user');
          console.error('Delete error:', err);
        }
      });
    }
  }

  getRoleBadgeClass(role: string): string {
    const roleMap: Record<string, string> = {
      'Admin': 'role-admin',
      'Manager': 'role-manager',
      'Receptionist': 'role-receptionist',
      'Housekeeping': 'role-housekeeping',
      'Customer': 'role-customer'
    };
    return roleMap[role] || 'role-default';
  }
}
