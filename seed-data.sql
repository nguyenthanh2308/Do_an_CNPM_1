-- =====================================================
-- SEED DATA FOR HOTEL MANAGEMENT SYSTEM
-- =====================================================
USE hotel_app;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- =====================================================
-- 1. USERS (Backoffice Staff)
-- =====================================================
-- Password for all users: "password123" (hashed with BCrypt workFactor 12)
INSERT INTO users (username, password_hash, role, email, created_at) VALUES
('admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Admin', 'admin@hotel.com', NOW()),
('manager1', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Manager', 'manager@hotel.com', NOW()),
('receptionist1', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Receptionist', 'receptionist1@hotel.com', NOW()),
('receptionist2', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Receptionist', 'receptionist2@hotel.com', NOW()),
('housekeeper1', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Housekeeping', 'housekeeper1@hotel.com', NOW()),
('housekeeper2', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Housekeeping', 'housekeeper2@hotel.com', NOW());

-- =====================================================
-- 2. HOTELS
-- =====================================================
INSERT INTO hotels (name, address, timezone, created_at) VALUES
('Grand Hotel Saigon', '123 Nguyen Hue, District 1, Ho Chi Minh City', 'Asia/Ho_Chi_Minh', NOW()),
('Hanoi Luxury Hotel', '456 Hoan Kiem, Hanoi', 'Asia/Ho_Chi_Minh', NOW());

-- =====================================================
-- 3. AMENITIES
-- =====================================================
INSERT INTO amenities (name) VALUES
('Free WiFi'),
('Air Conditioning'),
('TV'),
('Mini Bar'),
('Safe Box'),
('Coffee Maker'),
('Hair Dryer'),
('Bathtub'),
('Shower'),
('Balcony'),
('City View'),
('Ocean View'),
('Mountain View'),
('King Size Bed'),
('Queen Size Bed'),
('Twin Beds'),
('Sofa'),
('Work Desk'),
('Iron'),
('Telephone');

-- =====================================================
-- 4. ROOM TYPES
-- =====================================================
INSERT INTO room_types (hotel_id, name, capacity, base_price, description, default_image_url, created_at) VALUES
-- Grand Hotel Saigon
(1, 'Standard Room', 2, 800000.00, 'Phòng tiêu chuẩn với đầy đủ tiện nghi cơ bản', '/images/rooms/standard.jpg', NOW()),
(1, 'Deluxe Room', 2, 1200000.00, 'Phòng cao cấp với view đẹp và tiện nghi nâng cao', '/images/rooms/deluxe.jpg', NOW()),
(1, 'Suite Room', 4, 2500000.00, 'Phòng suite rộng rãi với phòng khách riêng', '/images/rooms/suite.jpg', NOW()),
(1, 'Family Room', 4, 1800000.00, 'Phòng gia đình rộng rãi phù hợp cho 4 người', '/images/rooms/family.jpg', NOW()),
-- Hanoi Luxury Hotel
(2, 'Standard Room', 2, 750000.00, 'Phòng tiêu chuẩn tại Hà Nội', '/images/rooms/standard-hn.jpg', NOW()),
(2, 'Executive Suite', 3, 2800000.00, 'Suite cao cấp với không gian làm việc', '/images/rooms/executive.jpg', NOW());

-- =====================================================
-- 5. ROOM TYPE AMENITIES (N-N Mapping)
-- =====================================================
-- Standard Room (Saigon) - Basic amenities
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(1, 1), (1, 2), (1, 3), (1, 7), (1, 9), (1, 15), (1, 18), (1, 20);

-- Deluxe Room (Saigon) - More amenities
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(2, 1), (2, 2), (2, 3), (2, 4), (2, 5), (2, 6), (2, 7), (2, 8), (2, 9), (2, 10), (2, 11), (2, 14), (2, 17), (2, 18), (2, 19), (2, 20);

-- Suite Room (Saigon) - All amenities
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(3, 1), (3, 2), (3, 3), (3, 4), (3, 5), (3, 6), (3, 7), (3, 8), (3, 9), (3, 10), (3, 11), (3, 14), (3, 17), (3, 18), (3, 19), (3, 20);

-- Family Room (Saigon)
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(4, 1), (4, 2), (4, 3), (4, 4), (4, 6), (4, 7), (4, 9), (4, 10), (4, 16), (4, 17), (4, 18), (4, 20);

-- Standard Room (Hanoi)
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(5, 1), (5, 2), (5, 3), (5, 7), (5, 9), (5, 15), (5, 18), (5, 20);

-- Executive Suite (Hanoi)
INSERT INTO room_type_amenities (room_type_id, amenity_id) VALUES
(6, 1), (6, 2), (6, 3), (6, 4), (6, 5), (6, 6), (6, 7), (6, 8), (6, 9), (6, 10), (6, 13), (6, 14), (6, 17), (6, 18), (6, 19), (6, 20);

-- =====================================================
-- 6. ROOMS
-- =====================================================
-- Grand Hotel Saigon - Floor 1-5
INSERT INTO rooms (hotel_id, room_type_id, number, floor, status, created_at) VALUES
-- Floor 1 - Standard Rooms
(1, 1, '101', 1, 'Vacant', NOW()),
(1, 1, '102', 1, 'Vacant', NOW()),
(1, 1, '103', 1, 'Occupied', NOW()),
(1, 1, '104', 1, 'Vacant', NOW()),
-- Floor 2 - Standard & Deluxe
(1, 1, '201', 2, 'Vacant', NOW()),
(1, 1, '202', 2, 'Cleaning', NOW()),
(1, 2, '203', 2, 'Vacant', NOW()),
(1, 2, '204', 2, 'Occupied', NOW()),
-- Floor 3 - Deluxe Rooms
(1, 2, '301', 3, 'Vacant', NOW()),
(1, 2, '302', 3, 'Vacant', NOW()),
(1, 2, '303', 3, 'Vacant', NOW()),
(1, 2, '304', 3, 'Maintenance', NOW()),
-- Floor 4 - Family Rooms
(1, 4, '401', 4, 'Vacant', NOW()),
(1, 4, '402', 4, 'Occupied', NOW()),
(1, 4, '403', 4, 'Vacant', NOW()),
-- Floor 5 - Suite Rooms
(1, 3, '501', 5, 'Vacant', NOW()),
(1, 3, '502', 5, 'Occupied', NOW()),
(1, 3, '503', 5, 'Vacant', NOW());

-- Hanoi Luxury Hotel
INSERT INTO rooms (hotel_id, room_type_id, number, floor, status, created_at) VALUES
(2, 5, '101', 1, 'Vacant', NOW()),
(2, 5, '102', 1, 'Vacant', NOW()),
(2, 5, '103', 1, 'Occupied', NOW()),
(2, 6, '201', 2, 'Vacant', NOW()),
(2, 6, '202', 2, 'Vacant', NOW());

-- =====================================================
-- 7. RATE PLANS
-- =====================================================
INSERT INTO rateplans (room_type_id, name, type, free_cancel_until_hours, start_date, end_date, price, weekend_rule_json, created_at) VALUES
-- Grand Hotel Saigon - Standard Room
(1, 'Standard - Flexible', 'Flexible', 24, '2025-01-01', '2025-12-31', 800000.00, '{"weekend_multiplier": 1.2, "peak_days": [5, 6]}', NOW()),
(1, 'Standard - Non-Refundable', 'NonRefundable', NULL, '2025-01-01', '2025-12-31', 720000.00, '{"weekend_multiplier": 1.2, "peak_days": [5, 6]}', NOW()),

-- Grand Hotel Saigon - Deluxe Room
(2, 'Deluxe - Flexible', 'Flexible', 48, '2025-01-01', '2025-12-31', 1200000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW()),
(2, 'Deluxe - Non-Refundable', 'NonRefundable', NULL, '2025-01-01', '2025-12-31', 1080000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW()),

-- Grand Hotel Saigon - Suite Room
(3, 'Suite - Flexible', 'Flexible', 72, '2025-01-01', '2025-12-31', 2500000.00, '{"weekend_multiplier": 1.25, "peak_days": [5, 6]}', NOW()),
(3, 'Suite - Early Bird', 'NonRefundable', NULL, '2025-01-01', '2025-12-31', 2200000.00, '{"weekend_multiplier": 1.25, "peak_days": [5, 6]}', NOW()),

-- Grand Hotel Saigon - Family Room
(4, 'Family - Flexible', 'Flexible', 24, '2025-01-01', '2025-12-31', 1800000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW()),

-- Hanoi Luxury Hotel
(5, 'Standard - Flexible', 'Flexible', 24, '2025-01-01', '2025-12-31', 750000.00, '{"weekend_multiplier": 1.2, "peak_days": [5, 6]}', NOW()),
(6, 'Executive - Flexible', 'Flexible', 48, '2025-01-01', '2025-12-31', 2800000.00, '{"weekend_multiplier": 1.3, "peak_days": [5, 6]}', NOW());

-- =====================================================
-- 8. PROMOTIONS
-- =====================================================
INSERT INTO promotions (code, type, value, start_date, end_date, conditions_json, created_at) VALUES
('WELCOME2025', 'Percent', 15.00, '2025-01-01', '2025-01-31', '{"min_nights": 2, "applicable_room_types": [1,2,3,4,5,6]}', NOW()),
('EARLYBIRD', 'Percent', 20.00, '2025-01-01', '2025-12-31', '{"min_nights": 3, "booking_days_before": 30}', NOW()),
('LONGSTAY', 'Percent', 25.00, '2025-01-01', '2025-12-31', '{"min_nights": 7}', NOW()),
('FLASH500', 'Amount', 500000.00, '2025-06-01', '2025-06-30', '{"min_total": 2000000}', NOW()),
('WEEKEND50', 'Percent', 10.00, '2025-01-01', '2025-12-31', '{"weekend_only": true}', NOW()),
('NEWCUSTOMER', 'Percent', 10.00, '2025-01-01', '2025-12-31', '{"first_booking_only": true}', NOW());

-- =====================================================
-- 9. GUESTS
-- =====================================================
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

-- =====================================================
-- 10. BOOKINGS
-- =====================================================
INSERT INTO bookings (hotel_id, guest_id, check_in_date, check_out_date, status, total_amount, payment_status, rateplan_snapshot_json, created_at) VALUES
-- Confirmed bookings
(1, 1, '2025-12-05', '2025-12-08', 'Confirmed', 2400000.00, 'Paid', '{"rateplan_id": 1, "price": 800000, "type": "Flexible"}', '2025-11-28 10:30:00'),
(1, 2, '2025-12-10', '2025-12-15', 'Confirmed', 5400000.00, 'Paid', '{"rateplan_id": 3, "price": 1200000, "type": "Flexible"}', '2025-11-29 14:20:00'),
(1, 3, '2025-12-15', '2025-12-20', 'Confirmed', 11000000.00, 'Unpaid', '{"rateplan_id": 5, "price": 2500000, "type": "Flexible"}', '2025-12-01 09:15:00'),

-- Checked-in bookings (currently staying)
(1, 4, '2025-12-01', '2025-12-05', 'CheckedIn', 2880000.00, 'Paid', '{"rateplan_id": 2, "price": 720000, "type": "NonRefundable"}', '2025-11-25 16:45:00'),
(1, 5, '2025-12-02', '2025-12-06', 'CheckedIn', 5400000.00, 'Paid', '{"rateplan_id": 7, "price": 1800000, "type": "Flexible"}', '2025-11-26 11:30:00'),
(1, 6, '2025-11-30', '2025-12-05', 'CheckedIn', 11000000.00, 'Paid', '{"rateplan_id": 6, "price": 2200000, "type": "NonRefundable"}', '2025-11-20 13:20:00'),

-- Checked-out bookings (completed)
(1, 7, '2025-11-20', '2025-11-23', 'CheckedOut', 2160000.00, 'Paid', '{"rateplan_id": 2, "price": 720000, "type": "NonRefundable"}', '2025-11-15 10:00:00'),
(1, 8, '2025-11-25', '2025-11-28', 'CheckedOut', 3600000.00, 'Paid', '{"rateplan_id": 3, "price": 1200000, "type": "Flexible"}', '2025-11-18 15:30:00'),

-- Cancelled booking
(1, 9, '2025-12-20', '2025-12-25', 'Cancelled', 0.00, 'Refunded', '{"rateplan_id": 1, "price": 800000, "type": "Flexible"}', '2025-11-28 08:00:00'),

-- Pending bookings
(2, 10, '2025-12-08', '2025-12-12', 'Pending', 3000000.00, 'Unpaid', '{"rateplan_id": 8, "price": 750000, "type": "Flexible"}', '2025-12-02 14:30:00');

-- =====================================================
-- 11. BOOKING ROOMS
-- =====================================================
INSERT INTO booking_rooms (booking_id, room_id, price_per_night, nights) VALUES
-- Booking 1: Room 101, 3 nights
(1, 1, 800000.00, 3),

-- Booking 2: Room 203 (Deluxe), 5 nights
(2, 7, 1200000.00, 5),

-- Booking 3: Room 501 (Suite), 5 nights
(3, 16, 2500000.00, 5),

-- Booking 4: Room 103 (Standard, currently occupied), 4 nights
(4, 3, 720000.00, 4),

-- Booking 5: Room 402 (Family, currently occupied), 4 nights
(5, 14, 1800000.00, 4),

-- Booking 6: Room 502 (Suite, currently occupied), 5 nights
(6, 17, 2200000.00, 5),

-- Booking 7: Completed, unassigned room
(7, NULL, 720000.00, 3),

-- Booking 8: Room 204 (Deluxe, completed)
(8, 8, 1200000.00, 3),

-- Booking 9: Cancelled
(9, NULL, 800000.00, 5),

-- Booking 10: Pending
(10, NULL, 750000.00, 4);

-- =====================================================
-- 12. PAYMENTS
-- =====================================================
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

-- =====================================================
-- 13. INVOICES
-- =====================================================
INSERT INTO invoices (booking_id, number, pdf_url, created_at) VALUES
(1, 'INV-2025-0001', '/invoices/INV-2025-0001.pdf', '2025-11-28 10:40:00'),
(2, 'INV-2025-0002', '/invoices/INV-2025-0002.pdf', '2025-11-29 14:30:00'),
(4, 'INV-2025-0003', '/invoices/INV-2025-0003.pdf', '2025-11-25 16:55:00'),
(5, 'INV-2025-0004', '/invoices/INV-2025-0004.pdf', '2025-11-26 11:40:00'),
(6, 'INV-2025-0005', '/invoices/INV-2025-0005.pdf', '2025-11-20 13:30:00'),
(7, 'INV-2025-0006', '/invoices/INV-2025-0006.pdf', '2025-11-15 10:10:00'),
(8, 'INV-2025-0007', '/invoices/INV-2025-0007.pdf', '2025-11-18 15:40:00');

-- =====================================================
-- 14. HOUSEKEEPING TASKS
-- =====================================================
INSERT INTO housekeeping_tasks (room_id, assigned_to_user_id, due_time, status, notes, created_at) VALUES
-- Cleaning tasks for vacant rooms
(1, 5, '2025-12-03 10:00:00', 'Done', 'Đã dọn phòng sạch sẽ', '2025-12-03 08:00:00'),
(2, 5, '2025-12-03 11:00:00', 'Done', 'Đã dọn xong', '2025-12-03 08:00:00'),
(4, 6, '2025-12-03 10:30:00', 'InProgress', 'Đang dọn phòng', '2025-12-03 08:00:00'),

-- Cleaning task for room needing cleaning
(6, 5, '2025-12-03 14:00:00', 'Todo', 'Cần dọn sau khi khách check-out', '2025-12-03 08:00:00'),

-- Maintenance task
(12, NULL, '2025-12-04 09:00:00', 'Blocked', 'Máy lạnh bị hỏng, chờ thợ sửa', '2025-12-02 16:00:00'),

-- Regular cleaning for occupied rooms
(3, 6, '2025-12-03 15:00:00', 'Todo', 'Dọn phòng khách đang ở', '2025-12-03 08:00:00'),
(8, 5, '2025-12-03 15:30:00', 'Todo', 'Dọn phòng khách đang ở', '2025-12-03 08:00:00'),
(14, 6, '2025-12-03 16:00:00', 'Todo', 'Dọn phòng gia đình', '2025-12-03 08:00:00');

-- =====================================================
-- VERIFICATION QUERIES (Optional - for testing)
-- =====================================================
-- SELECT 'Users' as TableName, COUNT(*) as Count FROM users
-- UNION ALL SELECT 'Hotels', COUNT(*) FROM hotels
-- UNION ALL SELECT 'Amenities', COUNT(*) FROM amenities
-- UNION ALL SELECT 'Room Types', COUNT(*) FROM room_types
-- UNION ALL SELECT 'Rooms', COUNT(*) FROM rooms
-- UNION ALL SELECT 'Rate Plans', COUNT(*) FROM rateplans
-- UNION ALL SELECT 'Promotions', COUNT(*) FROM promotions
-- UNION ALL SELECT 'Guests', COUNT(*) FROM guests
-- UNION ALL SELECT 'Bookings', COUNT(*) FROM bookings
-- UNION ALL SELECT 'Booking Rooms', COUNT(*) FROM booking_rooms
-- UNION ALL SELECT 'Payments', COUNT(*) FROM payments
-- UNION ALL SELECT 'Invoices', COUNT(*) FROM invoices
-- UNION ALL SELECT 'Housekeeping Tasks', COUNT(*) FROM housekeeping_tasks;

SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================
-- COMPLETED: Seed data inserted successfully
-- =====================================================
-- Summary:
-- - 6 Users (1 Admin, 1 Manager, 2 Receptionists, 2 Housekeepers)
-- - 2 Hotels (Saigon & Hanoi)
-- - 20 Amenities
-- - 6 Room Types
-- - 22 Rooms total
-- - 9 Rate Plans
-- - 6 Promotions
-- - 10 Guests
-- - 10 Bookings (various statuses)
-- - 10 Booking Room assignments
-- - 9 Payments
-- - 7 Invoices
-- - 8 Housekeeping Tasks
-- =====================================================
