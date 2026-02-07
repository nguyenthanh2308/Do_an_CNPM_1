// Room Models
export interface Room {
    id: number;
    number: string;
    roomTypeId: number;
    roomTypeName: string;
    hotelId: number;
    hotelName: string;
    floor: number;
    status: string; // Available, Occupied, Reserved, Maintenance
    basePrice: number;
}

export interface RoomType {
    id: number;
    name: string;
    description: string;
    basePrice: number;
    maxOccupancy: number;
    bedType: string;
    size: number;
    amenities: Amenity[];
}

export interface Amenity {
    id: number;
    name: string;
    description: string;
    icon?: string;
}

export interface CreateRoom {
    number: string;
    roomTypeId: number;
    hotelId: number;
    floor: number;
    status?: string;
}

export interface UpdateRoom {
    number?: string;
    roomTypeId?: number;
    floor?: number;
    status?: string;
}

export interface RoomAvailability {
    roomId: number;
    roomNumber: string;
    roomTypeName: string;
    isAvailable: boolean;
    pricePerNight: number;
}
