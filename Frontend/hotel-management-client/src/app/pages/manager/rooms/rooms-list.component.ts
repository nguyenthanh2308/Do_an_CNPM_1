import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomApiService } from '../../../services/room-api.service';
import { Room } from '../../../models/room.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-rooms-list',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent, PaginationComponent],
  templateUrl: './rooms-list.component.html',
  styleUrls: ['./rooms-list.component.css']
})
export class RoomsListComponent implements OnInit {
  rooms = signal<Room[]>([]);
  loading = signal(false);

  // Pagination
  currentPage = signal(1);
  pageSize = 20;
  totalPages = signal(1);
  totalItems = signal(0);

  // Filters
  statusFilter = '';

  constructor(
    private roomApi: RoomApiService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loadRooms();
  }

  loadRooms(page = 1) {
    this.loading.set(true);
    this.currentPage.set(page);

    this.roomApi.getAllRooms(page, this.pageSize, this.statusFilter || undefined).subscribe({
      next: (response) => {
        this.rooms.set(response.items);
        this.totalPages.set(response.totalPages || Math.ceil(response.totalCount / this.pageSize));
        this.totalItems.set(response.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error('Failed to load rooms');
        console.error('Error loading rooms:', err);
      }
    });
  }

  onFilterChange() {
    this.loadRooms(1); // Reset to first page on filter change
  }

  changeRoomStatus(room: Room, newStatus: string) {
    if (confirm(`Change room ${room.number} status to ${newStatus}?`)) {
      this.roomApi.updateRoomStatus(room.id, newStatus).subscribe({
        next: () => {
          this.toastService.success(`Room ${room.number} status updated to ${newStatus}`);
          this.loadRooms(this.currentPage());
        },
        error: (err) => {
          this.toastService.error('Failed to update room status');
          console.error('Status update error:', err);
        }
      });
    }
  }

  deleteRoom(room: Room) {
    if (confirm(`Are you sure you want to delete room ${room.number}?`)) {
      this.roomApi.deleteRoom(room.id).subscribe({
        next: () => {
          this.toastService.success(`Room ${room.number} deleted successfully`);
          this.loadRooms(this.currentPage());
        },
        error: (err) => {
          this.toastService.error('Failed to delete room');
          console.error('Delete error:', err);
        }
      });
    }
  }

  getStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Available': 'status-available',
      'Occupied': 'status-occupied',
      'Reserved': 'status-reserved',
      'Maintenance': 'status-maintenance'
    };
    return statusMap[status] || 'status-default';
  }

  getStatusIcon(status: string): string {
    const iconMap: Record<string, string> = {
      'Available': '✓',
      'Occupied': '●',
      'Reserved': '◐',
      'Maintenance': '⚠'
    };
    return iconMap[status] || '○';
  }
}
