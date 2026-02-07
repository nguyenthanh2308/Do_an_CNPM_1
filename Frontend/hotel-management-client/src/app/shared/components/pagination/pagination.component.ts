import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-pagination',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="pagination">
      <button
        class="pagination-btn"
        (click)="onPageChange(1)"
        [disabled]="currentPage === 1">
        First
      </button>

      <button
        class="pagination-btn"
        (click)="onPageChange(currentPage - 1)"
        [disabled]="currentPage === 1">
        Previous
      </button>

      <span class="pagination-info">
        Page {{ currentPage }} of {{ totalPages }}
        <span class="pagination-total">({{ totalItems }} total)</span>
      </span>

      <button
        class="pagination-btn"
        (click)="onPageChange(currentPage + 1)"
        [disabled]="currentPage === totalPages">
        Next
      </button>

      <button
        class="pagination-btn"
        (click)="onPageChange(totalPages)"
        [disabled]="currentPage === totalPages">
        Last
      </button>
    </div>
  `,
    styles: [`
    .pagination {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 15px 0;
      justify-content: center;
    }

    .pagination-btn {
      padding: 8px 16px;
      border: 1px solid #d1d5db;
      background-color: white;
      border-radius: 6px;
      cursor: pointer;
      font-size: 14px;
      transition: all 0.2s;
    }

    .pagination-btn:hover:not(:disabled) {
      background-color: #f3f4f6;
      border-color: #9ca3af;
    }

    .pagination-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .pagination-info {
      font-size: 14px;
      color: #4b5563;
      padding: 0 10px;
    }

    .pagination-total {
      color: #9ca3af;
      font-size: 12px;
    }
  `]
})
export class PaginationComponent {
    @Input() currentPage = 1;
    @Input() totalPages = 1;
    @Input() totalItems = 0;
    @Output() pageChange = new EventEmitter<number>();

    onPageChange(page: number) {
        if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
            this.pageChange.emit(page);
        }
    }
}
