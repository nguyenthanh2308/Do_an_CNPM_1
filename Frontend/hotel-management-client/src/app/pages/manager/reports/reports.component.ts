import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportApiService } from '../../../services/report-api.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ToastService } from '../../../shared/services/toast.service';

interface ReportDateRange {
  startDate: string;
  endDate: string;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {
  loading = signal(false);

  // Date range for reports
  dateRange = signal<ReportDateRange>({
    startDate: this.getDefaultStartDate(),
    endDate: this.getDefaultEndDate()
  });

  // Report data
  revenueData = signal<any>(null);
  occupancyData = signal<any>(null);

  constructor(
    private reportApi: ReportApiService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loadReports();
  }

  loadReports() {
    this.loading.set(true);
    const { startDate, endDate } = this.dateRange();

    Promise.all([
      this.loadRevenueReport(startDate, endDate),
      this.loadOccupancyReport(startDate, endDate)
    ]).then(() => {
      this.loading.set(false);
    }).catch(() => {
      this.loading.set(false);
    });
  }

  private async loadRevenueReport(startDate: string, endDate: string) {
    try {
      const response = await this.reportApi.getRevenueReport(new Date(startDate), new Date(endDate)).toPromise();
      this.revenueData.set(response?.data);
    } catch (error) {
      console.error('Error loading revenue report:', error);
      this.toastService.error('Failed to load revenue report');
    }
  }

  private async loadOccupancyReport(startDate: string, endDate: string) {
    try {
      const response = await this.reportApi.getOccupancyReport(new Date(startDate), new Date(endDate)).toPromise();
      this.occupancyData.set(response?.data);
    } catch (error) {
      console.error('Error loading occupancy report:', error);
      this.toastService.error('Failed to load occupancy report');
    }
  }

  onDateRangeChange() {
    this.loadReports();
  }

  exportRevenueCsv() {
    this.toastService.info('CSV export feature coming soon');
  }

  private getDefaultStartDate(): string {
    const date = new Date();
    date.setDate(date.getDate() - 30); // Last 30 days
    return date.toISOString().split('T')[0];
  }

  private getDefaultEndDate(): string {
    return new Date().toISOString().split('T')[0];
  }
}
