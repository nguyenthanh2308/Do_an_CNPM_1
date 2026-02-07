import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Payment, Invoice, CreatePayment, RefundPayment } from '../models/payment.model';
import { ApiResponse } from '../models/common.model';

@Injectable({
    providedIn: 'root'
})
export class PaymentApiService {
    private readonly paymentUrl = `${environment.apiUrl}/payments`;
    private readonly invoiceUrl = `${environment.apiUrl}/invoices`;

    constructor(private http: HttpClient) { }

    // ========== Payment Operations ==========

    /**
     * Get payments for a booking
     */
    getPaymentsByBooking(bookingId: number): Observable<ApiResponse<Payment[]>> {
        return this.http.get<ApiResponse<Payment[]>>(`${this.paymentUrl}/booking/${bookingId}`);
    }

    /**
     * Create payment
     */
    createPayment(payment: CreatePayment): Observable<ApiResponse<Payment>> {
        return this.http.post<ApiResponse<Payment>>(this.paymentUrl, payment);
    }

    /**
     * Refund payment
     */
    refundPayment(refund: RefundPayment): Observable<ApiResponse<Payment>> {
        return this.http.post<ApiResponse<Payment>>(
            `${this.paymentUrl}/${refund.paymentId}/refund`,
            refund
        );
    }

    /**
     * Get payment by ID
     */
    getPaymentById(id: number): Observable<ApiResponse<Payment>> {
        return this.http.get<ApiResponse<Payment>>(`${this.paymentUrl}/${id}`);
    }

    // ========== Invoice Operations ==========

    /**
     * Get invoice for booking
     */
    getInvoiceByBooking(bookingId: number): Observable<ApiResponse<Invoice>> {
        return this.http.get<ApiResponse<Invoice>>(`${this.invoiceUrl}/booking/${bookingId}`);
    }

    /**
     * Generate invoice for booking
     */
    generateInvoice(bookingId: number): Observable<ApiResponse<Invoice>> {
        return this.http.post<ApiResponse<Invoice>>(`${this.invoiceUrl}/generate`, {
            bookingId
        });
    }

    /**
     * Download invoice PDF (returns blob)
     */
    downloadInvoice(invoiceId: number): Observable<Blob> {
        return this.http.get(`${this.invoiceUrl}/${invoiceId}/pdf`, {
            responseType: 'blob'
        });
    }
}
