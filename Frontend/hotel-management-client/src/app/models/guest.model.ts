// Guest Models
export interface Guest {
    id: number;
    fullName: string;
    email: string;
    phone: string;
    identityNumber: string;
    address?: string;
    dateOfBirth?: Date;
    nationality?: string;
    createdAt: Date;
}

export interface CreateGuest {
    fullName: string;
    email: string;
    phone: string;
    identityNumber: string;
    address?: string;
    dateOfBirth?: Date;
    nationality?: string;
}

export interface UpdateGuest {
    fullName?: string;
    email?: string;
    phone?: string;
    address?: string;
    dateOfBirth?: Date;
    nationality?: string;
}
