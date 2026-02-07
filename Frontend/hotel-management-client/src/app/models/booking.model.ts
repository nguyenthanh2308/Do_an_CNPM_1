// Booking Models
export interface Booking {
    id: number;
    hotelId: number;
    hotelName: string;
    guestId: number;
    guestName: string;
    checkInDate: Date;
    checkOutDate: Date;
    status: string; // Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
    paymentStatus: string; // Unpaid, Partial, Paid
    totalAmount: number;
    createdAt: Date;
}

export interface BookingDetail extends Booking {
    rooms: Room[];
    payments: Payment[];
    guest: Guest;
}

export interface CreateBooking {
    hotelId: number;
    guestId: number;
    checkInDate: Date;
    checkOutDate: Date;
    roomIds: number[];
    promotionId?: number;
    specialRequests?: string;
}

export interface UpdateBooking {
    checkInDate?: Date;
    checkOutDate?: Date;
    status?: string;
}

// Supporting interfaces
interface Room {
    id: number;
    number: string;
    roomTypeName: string;
}

interface Payment {
    id: number;
    amount: number;
    method: string;
    paidAt: Date;
}

interface Guest {
    id: number;
    fullName: string;
    email: string;
    phone: string;
}
