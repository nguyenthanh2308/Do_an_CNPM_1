import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingApiService } from '../../../services/booking-api.service';
import { RoomApiService } from '../../../services/room-api.service';
import { ReportApiService } from '../../../services/report-api.service';
import { Booking } from '../../../models/booking.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ToastService } from '../../../shared/services/toast.service';

interface DashboardStats {
  totalRevenue: number;
  totalBookings: number;
  todayCheckIns: number;
  availableRooms: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  stats = signal<DashboardStats>({
    totalRevenue: 0,
    totalBookings: 0,
    todayCheckIns: 0,
    availableRooms: 0
  });

  recentBookings = signal<Booking[]>([]);
  loading = signal(false);
  currentDate = new Date();

  constructor(
    private bookingApi: BookingApiService,
    private roomApi: RoomApiService,
    private reportApi: ReportApiService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.loading.set(true);

    // Load stats in parallel
    Promise.all([
      this.loadBookingStats(),
      this.loadRoomStats(),
      this.loadRecentBookings()
    ]).then(() => {
      this.loading.set(false);
    }).catch(error => {
      this.loading.set(false);
      this.toastService.error('Failed to load dashboard data');
      console.error('Dashboard error:', error);
    });
  }

  private async loadBookingStats() {
    try {
      // Get all bookings to calculate stats
      const response = await this.bookingApi.getAll(1, 100).toPromise();

      if (response && response.items) {
        const bookings = response.items;

        // Calculate total revenue
        const totalRevenue = bookings.reduce((sum, b) => sum + (b.totalAmount || 0), 0);

        // Count today's check-ins
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const todayCheckIns = bookings.filter(b => {
          const checkInDate = new Date(b.checkInDate);
          checkInDate.setHours(0, 0, 0, 0);
          return checkInDate.getTime() === today.getTime();
        }).length;

        this.stats.update(s => ({
          ...s,
          totalRevenue,
          totalBookings: response.totalCount,
          todayCheckIns
        }));
      }
    } catch (error) {
      console.error('Error loading booking stats:', error);
    }
  }

  private async loadRoomStats() {
    try {
      // Get available rooms
      const response = await this.roomApi.getAllRooms(1, 100, 'Available').toPromise();

      if (response) {
        this.stats.update(s => ({
          ...s,
          availableRooms: response.totalCount
        }));
      }
    } catch (error) {
      console.error('Error loading room stats:', error);
    }
  }

  private async loadRecentBookings() {
    try {
      const response = await this.bookingApi.getAll(1, 10).toPromise();

      if (response && response.items) {
        this.recentBookings.set(response.items);
      }
    } catch (error) {
      console.error('Error loading recent bookings:', error);
    }
  }

  getStatusClass(status: string): string {
    const statusMap: Record<string, string> = {
      'Pending': 'status-pending',
      'Confirmed': 'status-confirmed',
      'CheckedIn': 'status-checkedin',
      'CheckedOut': 'status-checkedout',
      'Cancelled': 'status-cancelled'
    };
    return statusMap[status] || 'status-default';
  }

  refresh() {
    this.loadDashboardData();
    this.toastService.info('Dashboard refreshed');
  }
}
