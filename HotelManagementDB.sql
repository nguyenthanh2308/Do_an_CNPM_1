-- =====================================================
-- HOTEL MANAGEMENT SYSTEM - COMPLETE DATABASE SCRIPT
-- =====================================================
-- This script creates the database, tables with all latest updates,
-- and populates it with initial seed data.
-- =====================================================

CREATE DATABASE IF NOT EXISTS hotel_app
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;
USE hotel_app;

-- Ensure engine & mode
SET NAMES utf8mb4;
SET sql_notes = 0;
SET FOREIGN_KEY_CHECKS = 0;

-- =====================================================
-- 1. TABLE DEFINITIONS
-- =====================================================

-- 1.1 Users (Tables for auth)
CREATE TABLE users (
  id                BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  username          VARCHAR(64)  NOT NULL,
  password_hash     VARCHAR(255) NOT NULL,
  role              ENUM('Admin','Manager','Receptionist','Housekeeping') NOT NULL,
  email             VARCHAR(128) NULL,
  created_at        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_users_username (username),
  UNIQUE KEY uq_users_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.2 Hotels
CREATE TABLE hotels (
  id          BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  name        VARCHAR(128) NOT NULL,
  address     VARCHAR(255) NULL,
  timezone    VARCHAR(64)  NOT NULL DEFAULT 'Asia/Ho_Chi_Minh',
  created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.3 Amenities
CREATE TABLE amenities (
  id    BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  name  VARCHAR(64) NOT NULL,
  UNIQUE KEY uq_amenities_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.4 RoomTypes
CREATE TABLE room_types (
  id                BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  hotel_id          BIGINT UNSIGNED NOT NULL,
  name              VARCHAR(128) NOT NULL,
  capacity          TINYINT UNSIGNED NOT NULL DEFAULT 2,
  base_price        DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  description       TEXT NULL,
  default_image_url VARCHAR(255) NULL,
  created_at        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_roomtypes_hotel
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  UNIQUE KEY uq_roomtypes_hotel_name (hotel_id, name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.5 Room Type Amenities (N-N)
CREATE TABLE room_type_amenities (
  room_type_id  BIGINT UNSIGNED NOT NULL,
  amenity_id    BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (room_type_id, amenity_id),
  CONSTRAINT fk_rta_roomtype
    FOREIGN KEY (room_type_id) REFERENCES room_types(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_rta_amenity
    FOREIGN KEY (amenity_id) REFERENCES amenities(id)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.6 Rooms
CREATE TABLE rooms (
  id           BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  hotel_id     BIGINT UNSIGNED NOT NULL,
  room_type_id BIGINT UNSIGNED NOT NULL,
  number       VARCHAR(16) NOT NULL,   -- Room number
  floor        SMALLINT     NULL,
  status       ENUM('Vacant','Occupied','Cleaning','Maintenance') NOT NULL DEFAULT 'Vacant',
  created_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_rooms_hotel
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_rooms_roomtype
    FOREIGN KEY (room_type_id) REFERENCES room_types(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  UNIQUE KEY uq_rooms_hotel_number (hotel_id, number),
  KEY idx_rooms_status (status),
  KEY idx_rooms_roomtype (room_type_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.7 RatePlans
CREATE TABLE rateplans (
  id                         BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  room_type_id               BIGINT UNSIGNED NOT NULL,
  name                       VARCHAR(128) NOT NULL,
  type                       ENUM('Flexible','NonRefundable') NOT NULL,
  free_cancel_until_hours    INT NULL,   -- Only for Flexible
  start_date                 DATE NOT NULL,
  end_date                   DATE NOT NULL,
  price                      DECIMAL(12,2) NOT NULL,
  weekend_rule_json          JSON NULL,  -- Peak/Weekend pricing rules
  created_at                 DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_rateplans_roomtype
    FOREIGN KEY (room_type_id) REFERENCES room_types(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  KEY idx_rateplans_roomtype (room_type_id),
  KEY idx_rateplans_daterange (start_date, end_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.8 Promotions
CREATE TABLE promotions (
  id               BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  code             VARCHAR(32) NOT NULL,
  type             ENUM('Percent','Amount') NOT NULL,
  value            DECIMAL(12,2) NOT NULL,
  start_date       DATE NOT NULL,
  end_date         DATE NOT NULL,
  conditions_json  JSON NULL,
  created_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_promotions_code (code),
  KEY idx_promotions_daterange (start_date, end_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.9 Guests
CREATE TABLE guests (
  id         BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  full_name  VARCHAR(128) NOT NULL,
  email      VARCHAR(128) NULL,
  phone      VARCHAR(32)  NULL,
  id_number  VARCHAR(32)  NULL,  -- ID Card/Passport
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_guests_email (email),
  KEY idx_guests_phone (phone)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.10 Bookings
CREATE TABLE bookings (
  id                       BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  hotel_id                 BIGINT UNSIGNED NOT NULL,
  guest_id                 BIGINT UNSIGNED NOT NULL,
  check_in_date            DATE NOT NULL,
  check_out_date           DATE NOT NULL,
  status                   ENUM('Pending','Confirmed','CheckedIn','CheckedOut','Cancelled') NOT NULL DEFAULT 'Pending',
  total_amount             DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  payment_status           ENUM('Unpaid','Paid','Refunded','Failed') NOT NULL DEFAULT 'Unpaid',
  discount_amount          DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  rateplan_snapshot_json   JSON NULL, -- Snapshot of pricing/cancel policy at booking time
  promotion_id             BIGINT UNSIGNED NULL,
  created_at               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  cancelled_at             DATETIME NULL,
  modified_at              DATETIME NULL,
  checkin_actual_date      DATETIME NULL,
  checkout_actual_date     DATETIME NULL,
  CONSTRAINT fk_bookings_hotel
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_bookings_guest
    FOREIGN KEY (guest_id) REFERENCES guests(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_bookings_promotion 
    FOREIGN KEY (promotion_id) REFERENCES promotions(id) 
    ON UPDATE CASCADE ON DELETE SET NULL,
  KEY idx_bookings_hotel_dates (hotel_id, check_in_date, check_out_date),
  KEY idx_bookings_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.11 BookingRooms (N-N with extra data)
CREATE TABLE booking_rooms (
  id               BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  booking_id       BIGINT UNSIGNED NOT NULL,
  room_id          BIGINT UNSIGNED NULL, -- Can be NULL before check-in assignment
  price_per_night  DECIMAL(12,2) NOT NULL,
  nights           INT NOT NULL,
  CONSTRAINT fk_brooms_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_brooms_room
    FOREIGN KEY (room_id) REFERENCES rooms(id)
    ON UPDATE CASCADE ON DELETE SET NULL,
  KEY idx_brooms_booking (booking_id),
  KEY idx_brooms_room (room_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.12 Payments
CREATE TABLE payments (
  id          BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  booking_id  BIGINT UNSIGNED NOT NULL,
  method      ENUM('Mock','PayAtProperty') NOT NULL,
  amount      DECIMAL(12,2) NOT NULL,
  txn_code    VARCHAR(64) NULL,
  status      ENUM('Unpaid','Paid','Refunded','Failed') NOT NULL DEFAULT 'Unpaid',
  created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_payments_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  KEY idx_payments_booking (booking_id),
  KEY idx_payments_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.13 Invoices
CREATE TABLE invoices (
  id          BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  booking_id  BIGINT UNSIGNED NOT NULL,
  number      VARCHAR(32) NOT NULL,
  amount      DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  pdf_url     VARCHAR(255) NULL,
  created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_invoices_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  UNIQUE KEY uq_invoices_number (number),
  UNIQUE KEY uq_invoices_booking (booking_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.14 Housekeeping Tasks
CREATE TABLE housekeeping_tasks (
  id                   BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  room_id              BIGINT UNSIGNED NOT NULL,
  assigned_to_user_id  BIGINT UNSIGNED NULL, -- Housekeeping staff
  task_type            VARCHAR(20) NOT NULL DEFAULT 'Cleaning',
  priority             VARCHAR(20) NOT NULL DEFAULT 'Normal',
  scheduled_at         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  completed_at         DATETIME NULL,
  status               VARCHAR(50) NOT NULL DEFAULT 'Pending',
  notes                TEXT NULL,
  booking_id           BIGINT UNSIGNED NULL,
  created_at           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_hk_room
    FOREIGN KEY (room_id) REFERENCES rooms(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_hk_user
    FOREIGN KEY (assigned_to_user_id) REFERENCES users(id)
    ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT fk_hk_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON DELETE SET NULL,
  KEY idx_hk_room_status (room_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- =====================================================
-- 2. SEED DATA
-- =====================================================

-- 2.1 USERS
INSERT INTO users (username, password_hash, role, email, created_at) VALUES
('admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Admin', 'admin@hotel.com', NOW()),
('manager1', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Manager', 'manager@hotel.com', NOW()),
('receptionist1', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Receptionist', 'receptionist1@hotel.com', NOW()),
('receptionist2', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Receptionist', 'receptionist2@hotel.com', NOW()),
('housekeeper1', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Housekeeping', 'housekeeper1@hotel.com', NOW()),
('housekeeper2', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Housekeeping', 'housekeeper2@hotel.com', NOW());

-- 2.2 HOTELS
INSERT INTO hotels (name, address, timezone, created_at) VALUES
('Grand Hotel Saigon', '123 Nguyen Hue, District 1, Ho Chi Minh City', 'Asia/Ho_Chi_Minh', NOW()),
('Hanoi Luxury Hotel', '456 Hoan Kiem, Hanoi', 'Asia/Ho_Chi_Minh', NOW());

-- 2.3 AMENITIES
INSERT INTO amenities (name) VALUES
('Free WiFi'),('Air Conditioning'),('TV'),('Mini Bar'),('Safe Box'),
('Coffee Maker'),('Hair Dryer'),('Bathtub'),('Shower'),('Balcony'),
('City View'),('Ocean View'),('Mountain View'),('King Size Bed'),('Queen Size Bed'),
('Twin Beds'),('Sofa'),('Work Desk'),('Iron'),('Telephone');

-- 2.4 ROOM TYPES
INSERT INTO room_types (hotel_id, name, capacity, base_price, description, default_image_url, created_at) VALUES
(1, 'Standard Room', 2, 800000.00, 'Phòng tiêu chuẩn với đầy đủ tiện nghi cơ bản', '/images/rooms/standard.jpg', NOW()),
(1, 'Deluxe Room', 2, 1200000.00, 'Phòng cao cấp với view đẹp và tiện nghi nâng cao', '/images/rooms/deluxe.jpg', NOW()),
(1, 'Suite Room', 4, 2500000.00, 'Phòng suite rộng rãi với phòng khách riêng', '/images/rooms/suite.jpg', NOW()),
(1, 'Family Room', 4, 1800000.00, 'Phòng gia đình rộng rãi phù hợp cho 4 người', '/images/rooms/family.jpg', NOW()),
(2, 'Standard Room', 2, 750000.00, 'Phòng tiêu chuẩn tại Hà Nội', '/images/rooms/standard-hn.jpg', NOW()),
(2, 'Executive Suite', 3, 2800000.00, 'Suite cao cấp với không gian làm việc', '/images/rooms/executive.jpg', NOW());

-- 2.5 ROOM TYPE AMENITIES
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(1, 1), (1, 2), (1, 3), (1, 7), (1, 9), (1, 15), (1, 18), (1, 20),
(2, 1), (2, 2), (2, 3), (2, 4), (2, 5), (2, 6), (2, 7), (2, 8), (2, 9), (2, 10), (2, 11), (2, 14), (2, 17), (2, 18), (2, 19), (2, 20),
(3, 1), (3, 2), (3, 3), (3, 4), (3, 5), (3, 6), (3, 7), (3, 8), (3, 9), (3, 10), (3, 11), (3, 14), (3, 17), (3, 18), (3, 19), (3, 20),
(4, 1), (4, 2), (4, 3), (4, 4), (4, 6), (4, 7), (4, 9), (4, 10), (4, 16), (4, 17), (4, 18), (4, 20),
(5, 1), (5, 2), (5, 3), (5, 7), (5, 9), (5, 15), (5, 18), (5, 20),
(6, 1), (6, 2), (6, 3), (6, 4), (6, 5), (6, 6), (6, 7), (6, 8), (6, 9), (6, 10), (6, 13), (6, 14), (6, 17), (6, 18), (6, 19), (6, 20);

-- 2.6 ROOMS
INSERT INTO rooms (hotel_id, room_type_id, number, floor, status, created_at) VALUES
(1, 1, '101', 1, 'Vacant', NOW()), (1, 1, '102', 1, 'Vacant', NOW()), (1, 1, '103', 1, 'Occupied', NOW()), (1, 1, '104', 1, 'Vacant', NOW()),
(1, 1, '201', 2, 'Vacant', NOW()), (1, 1, '202', 2, 'Cleaning', NOW()), (1, 2, '203', 2, 'Vacant', NOW()), (1, 2, '204', 2, 'Occupied', NOW()),
(1, 2, '301', 3, 'Vacant', NOW()), (1, 2, '302', 3, 'Vacant', NOW()), (1, 2, '303', 3, 'Vacant', NOW()), (1, 2, '304', 3, 'Maintenance', NOW()),
(1, 4, '401', 4, 'Vacant', NOW()), (1, 4, '402', 4, 'Occupied', NOW()), (1, 4, '403', 4, 'Vacant', NOW()),
(1, 3, '501', 5, 'Vacant', NOW()), (1, 3, '502', 5, 'Occupied', NOW()), (1, 3, '503', 5, 'Vacant', NOW()),
(2, 5, '101', 1, 'Vacant', NOW()), (2, 5, '102', 1, 'Vacant', NOW()), (2, 5, '103', 1, 'Occupied', NOW()),
(2, 6, '201', 2, 'Vacant', NOW()), (2, 6, '202', 2, 'Vacant', NOW());

-- 2.7 RATE PLANS
INSERT INTO rateplans (room_type_id, name, type, free_cancel_until_hours, start_date, end_date, price, weekend_rule_json, created_at) VALUES
(1, 'Standard - Flexible', 'Flexible', 24, '2025-01-01', '2025-12-31', 800000.00, '{"weekend_multiplier": 1.2, "peak_days": [5, 6]}', NOW()),
(1, 'Standard - Non-Refundable', 'NonRefundable', NULL, '2025-01-01', '2025-12-31', 720000.00, '{"weekend_multiplier": 1.2, "peak_days": [5, 6]}', NOW()),
(2, 'Deluxe - Flexible', 'Flexible', 48, '2025-01-01', '2025-12-31', 1200000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW()),
(2, 'Deluxe - Non-Refundable', 'NonRefundable', NULL, '2025-01-01', '2025-12-31', 1080000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW()),
(3, 'Suite - Flexible', 'Flexible', 72, '2025-01-01', '2025-12-31', 2500000.00, '{"weekend_multiplier": 1.25, "peak_days": [5, 6]}', NOW()),
(3, 'Suite - Early Bird', 'NonRefundable', NULL, '2025-01-01', '2025-12-31', 2200000.00, '{"weekend_multiplier": 1.25, "peak_days": [5, 6]}', NOW()),
(4, 'Family - Flexible', 'Flexible', 24, '2025-01-01', '2025-12-31', 1800000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW()),
(5, 'Standard - Flexible', 'Flexible', 24, '2025-01-01', '2025-12-31', 750000.00, '{"weekend_multiplier": 1.2, "peak_days": [5, 6]}', NOW()),
(6, 'Executive - Flexible', 'Flexible', 48, '2025-01-01', '2025-12-31', 2800000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW());

-- 2.8 PROMOTIONS
INSERT INTO promotions (code, type, value, start_date, end_date, conditions_json, created_at) VALUES
('WELCOME2025', 'Percent', 15.00, '2025-01-01', '2025-01-31', '{"min_nights": 2, "applicable_room_types": [1,2,3,4,5,6]}', NOW()),
('EARLYBIRD', 'Percent', 20.00, '2025-01-01', '2025-12-31', '{"min_nights": 3, "booking_days_before": 30}', NOW()),
('LONGSTAY', 'Percent', 25.00, '2025-01-01', '2025-12-31', '{"min_nights": 7}', NOW()),
('FLASH500', 'Amount', 500000.00, '2025-06-01', '2025-06-30', '{"min_total": 2000000}', NOW()),
('WEEKEND50', 'Percent', 10.00, '2025-01-01', '2025-12-31', '{"weekend_only": true}', NOW()),
('NEWCUSTOMER', 'Percent', 10.00, '2025-01-01', '2025-12-31', '{"first_booking_only": true}', NOW());

-- 2.9 GUESTS
INSERT INTO guests (full_name, email, phone, id_number, created_at) VALUES
('Nguyễn Văn An', 'nguyenvanan@gmail.com', '0901234567', '079012345678', NOW()),
('Trần Thị Bình', 'tranthib@gmail.com', '0912345678', '079123456789', NOW()),
('Lê Văn Cường', 'levancuong@gmail.com', '0923456789', '079234567890', NOW()),
('Phạm Thị Dung', 'phamthidung@gmail.com', '0934567890', '079345678901', NOW()),
('Hoàng Văn Hùng', 'hoangvanhung@gmail.com', '0945678901', '079456789012', NOW()),
('Đỗ Thị Mai', 'dothimai@gmail.com', '0956789012', '079567890123', NOW()),
('Vũ Văn Nam', 'vuvannam@gmail.com', '0967890123', '079678901234', NOW()),
('Bùi Thị Oanh', 'buithioanh@gmail.com', '0978901234', '079789012345', NOW()),
('Đinh Văn Phong', 'dinhvanphong@gmail.com', '0989012345', '079890123456', NOW()),
('Ngô Thị Quỳnh', 'ngothiquynh@gmail.com', '0990123456', '079901234567', NOW());

-- 2.10 BOOKINGS
INSERT INTO bookings (hotel_id, guest_id, check_in_date, check_out_date, status, total_amount, payment_status, rateplan_snapshot_json, created_at) VALUES
(1, 1, '2025-12-05', '2025-12-08', 'Confirmed', 2400000.00, 'Paid', '{"rateplan_id": 1, "price": 800000, "type": "Flexible"}', '2025-11-28 10:30:00'),
(1, 2, '2025-12-10', '2025-12-15', 'Confirmed', 5400000.00, 'Paid', '{"rateplan_id": 3, "price": 1200000, "type": "Flexible"}', '2025-11-29 14:20:00'),
(1, 3, '2025-12-15', '2025-12-20', 'Confirmed', 11000000.00, 'Unpaid', '{"rateplan_id": 5, "price": 2500000, "type": "Flexible"}', '2025-12-01 09:15:00'),
(1, 4, '2025-12-01', '2025-12-05', 'CheckedIn', 2880000.00, 'Paid', '{"rateplan_id": 2, "price": 720000, "type": "NonRefundable"}', '2025-11-25 16:45:00'),
(1, 5, '2025-12-02', '2025-12-06', 'CheckedIn', 5400000.00, 'Paid', '{"rateplan_id": 7, "price": 1800000, "type": "Flexible"}', '2025-11-26 11:30:00'),
(1, 6, '2025-11-30', '2025-12-05', 'CheckedIn', 11000000.00, 'Paid', '{"rateplan_id": 6, "price": 2200000, "type": "NonRefundable"}', '2025-11-20 13:20:00'),
(1, 7, '2025-11-20', '2025-11-23', 'CheckedOut', 2160000.00, 'Paid', '{"rateplan_id": 2, "price": 720000, "type": "NonRefundable"}', '2025-11-15 10:00:00'),
(1, 8, '2025-11-25', '2025-11-28', 'CheckedOut', 3600000.00, 'Paid', '{"rateplan_id": 3, "price": 1200000, "type": "Flexible"}', '2025-11-18 15:30:00'),
(1, 9, '2025-12-20', '2025-12-25', 'Cancelled', 0.00, 'Refunded', '{"rateplan_id": 1, "price": 800000, "type": "Flexible"}', '2025-11-28 08:00:00'),
(2, 10, '2025-12-08', '2025-12-12', 'Pending', 3000000.00, 'Unpaid', '{"rateplan_id": 8, "price": 750000, "type": "Flexible"}', '2025-12-02 14:30:00');

-- 2.11 BOOKING ROOMS
INSERT INTO booking_rooms (booking_id, room_id, price_per_night, nights) VALUES
(1, 1, 800000.00, 3), (2, 7, 1200000.00, 5), (3, 16, 2500000.00, 5),
(4, 3, 720000.00, 4), (5, 14, 1800000.00, 4), (6, 17, 2200000.00, 5),
(7, NULL, 720000.00, 3), (8, 8, 1200000.00, 3), (9, NULL, 800000.00, 5),
(10, NULL, 750000.00, 4);

-- 2.12 PAYMENTS
INSERT INTO payments (booking_id, method, amount, txn_code, status, created_at) VALUES
(1, 'Mock', 2400000.00, 'TXN2025110001', 'Paid', '2025-11-28 10:35:00'),
(2, 'Mock', 5400000.00, 'TXN2025110002', 'Paid', '2025-11-29 14:25:00'),
(3, 'PayAtProperty', 11000000.00, NULL, 'Unpaid', '2025-12-01 09:20:00'),
(4, 'Mock', 2880000.00, 'TXN2025110003', 'Paid', '2025-11-25 16:50:00'),
(5, 'Mock', 5400000.00, 'TXN2025110004', 'Paid', '2025-11-26 11:35:00'),
(6, 'Mock', 11000000.00, 'TXN2025110005', 'Paid', '2025-11-20 13:25:00'),
(7, 'Mock', 2160000.00, 'TXN2025110006', 'Paid', '2025-11-15 10:05:00'),
(8, 'Mock', 3600000.00, 'TXN2025110007', 'Paid', '2025-11-18 15:35:00'),
(9, 'Mock', 4000000.00, 'TXN2025110008', 'Refunded', '2025-11-28 08:05:00');

-- 2.13 INVOICES
INSERT INTO invoices (booking_id, number, amount, pdf_url, created_at) VALUES
(1, 'INV-2025-0001', 2400000.00, '/invoices/INV-2025-0001.pdf', '2025-11-28 10:40:00'),
(2, 'INV-2025-0002', 5400000.00, '/invoices/INV-2025-0002.pdf', '2025-11-29 14:30:00'),
(4, 'INV-2025-0003', 2880000.00, '/invoices/INV-2025-0003.pdf', '2025-11-25 16:55:00'),
(5, 'INV-2025-0004', 5400000.00, '/invoices/INV-2025-0004.pdf', '2025-11-26 11:40:00'),
(6, 'INV-2025-0005', 11000000.00, '/invoices/INV-2025-0005.pdf', '2025-11-20 13:30:00'),
(7, 'INV-2025-0006', 2160000.00, '/invoices/INV-2025-0006.pdf', '2025-11-15 10:10:00'),
(8, 'INV-2025-0007', 3600000.00, '/invoices/INV-2025-0007.pdf', '2025-11-18 15:40:00');

-- 2.14 HOUSEKEEPING TASKS (Legacy mappings: Todo->Pending, Done->Completed, Blocked->Cancelled, InProgress->Pending, due_time->scheduled_at)
INSERT INTO housekeeping_tasks (room_id, assigned_to_user_id, scheduled_at, status, notes, created_at) VALUES
(1, 5, '2025-12-03 10:00:00', 'Completed', 'Đã dọn phòng sạch sẽ', '2025-12-03 08:00:00'),
(2, 5, '2025-12-03 11:00:00', 'Completed', 'Đã dọn xong', '2025-12-03 08:00:00'),
(4, 6, '2025-12-03 10:30:00', 'Pending', 'Đang dọn phòng', '2025-12-03 08:00:00'),
(6, 5, '2025-12-03 14:00:00', 'Pending', 'Cần dọn sau khi khách check-out', '2025-12-03 08:00:00'),
(12, NULL, '2025-12-04 09:00:00', 'Cancelled', 'Máy lạnh bị hỏng, chờ thợ sửa', '2025-12-02 16:00:00'),
(3, 6, '2025-12-03 15:00:00', 'Pending', 'Dọn phòng khách đang ở', '2025-12-03 08:00:00'),
(8, 5, '2025-12-03 15:30:00', 'Pending', 'Dọn phòng khách đang ở', '2025-12-03 08:00:00'),
(14, 6, '2025-12-03 16:00:00', 'Pending', 'Dọn phòng gia đình', '2025-12-03 08:00:00');

SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================
-- END OF SCRIPT
-- =====================================================
