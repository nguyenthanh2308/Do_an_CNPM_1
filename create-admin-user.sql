-- =====================================================
-- TẠO TÀI KHOẢN ADMIN ĐẦU TIÊN
-- =====================================================
USE hotel_app;

-- Xóa user admin cũ nếu có
DELETE FROM users WHERE username = 'admin';

-- Tạo tài khoản admin mới
-- Username: admin
-- Password: password123
-- Password hash được tạo bằng BCrypt với workFactor 12
INSERT INTO users (username, password_hash, role, email, created_at) VALUES
('admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIq.Zu7Qle', 'Admin', 'admin@hotel.com', NOW());

-- Kiểm tra user đã được tạo
SELECT id, username, role, email, created_at FROM users WHERE username = 'admin';

-- =====================================================
-- HƯỚNG DẪN ĐĂNG NHẬP:
-- Username: admin
-- Password: password123
-- =====================================================

UPDATE users 
SET password_hash = '$2b$12$P2Rrh1RX3Vtz75wljXFUROpcaSqfevDt4l8fd0OGgjM7GggMhwv0.'
WHERE username = 'admin';
