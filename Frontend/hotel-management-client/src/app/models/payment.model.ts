// Payment Models
export interface Payment {
    id: number;
    bookingId: number;
    amount: number;
    method: string; // Cash, Card, BankTransfer
    status: string; // Pending, Completed, Failed, Refunded
    txnCode?: string;
    paidAt?: Date;
    createdAt: Date;
}

export interface Invoice {
    id: number;
    bookingId: number;
    number: string;
    amount: number;
    tax: number;
    discount: number;
    totalAmount: number;
    status: string; // Issued, Paid, Cancelled
    issuedAt: Date;
}

export interface CreatePayment {
    bookingId: number;
    amount: number;
    method: string;
    txnCode?: string;
}

export interface RefundPayment {
    paymentId: number;
    amount: number;
    reason: string;
}
