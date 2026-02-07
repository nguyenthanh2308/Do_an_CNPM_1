import { Injectable, signal } from '@angular/core';

export interface Toast {
    id: number;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
    duration?: number;
}

@Injectable({
    providedIn: 'root'
})
export class ToastService {
    // Using Angular 21 signals for reactive toast list
    private toastsSignal = signal<Toast[]>([]);
    public toasts = this.toastsSignal.asReadonly();

    private nextId = 1;

    /**
     * Show success toast
     */
    success(message: string, duration = 3000) {
        this.show(message, 'success', duration);
    }

    /**
     * Show error toast
     */
    error(message: string, duration = 5000) {
        this.show(message, 'error', duration);
    }

    /**
     * Show info toast
     */
    info(message: string, duration = 3000) {
        this.show(message, 'info', duration);
    }

    /**
     * Show warning toast
     */
    warning(message: string, duration = 4000) {
        this.show(message, 'warning', duration);
    }

    /**
     * Show toast with custom type and duration
     */
    private show(message: string, type: Toast['type'], duration: number) {
        const toast: Toast = {
            id: this.nextId++,
            message,
            type,
            duration
        };

        // Add toast to list
        this.toastsSignal.update(toasts => [...toasts, toast]);

        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => this.remove(toast.id), duration);
        }
    }

    /**
     * Manually remove toast
     */
    remove(id: number) {
        this.toastsSignal.update(toasts => toasts.filter(t => t.id !== id));
    }

    /**
     * Clear all toasts
     */
    clearAll() {
        this.toastsSignal.set([]);
    }
}
