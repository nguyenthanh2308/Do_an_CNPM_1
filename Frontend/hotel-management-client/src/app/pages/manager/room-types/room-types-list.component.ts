import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoomApiService } from '../../../services/room-api.service';
import { RoomType } from '../../../models/room.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-room-types-list',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent],
  templateUrl: './room-types-list.component.html',
  styleUrls: ['./room-types-list.component.css']
})
export class RoomTypesListComponent implements OnInit {
  roomTypes = signal<RoomType[]>([]);
  loading = signal(false);

  constructor(
    private roomApi: RoomApiService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loadRoomTypes();
  }

  loadRoomTypes() {
    this.loading.set(true);

    this.roomApi.getAllRoomTypes().subscribe({
      next: (response) => {
        if (response && response.data) {
          this.roomTypes.set(response.data);
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.loading.set(false);
        this.toastService.error('Failed to load room types');
        console.error('Error loading room types:', err);
      }
    });
  }

  deleteRoomType(roomType: RoomType) {
    this.toastService.info('Delete room type feature coming soon');
  }
}
