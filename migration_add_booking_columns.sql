-- Migration: Add missing columns to bookings table
-- Date: 2025-12-03
-- Description: Add all missing columns to match Booking entity model

USE hotel_app;

-- Check if columns exist before adding them
SET @dbname = DATABASE();
SET @tablename = 'bookings';

-- Add cancelled_at column if not exists
SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'cancelled_at');
SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE bookings ADD COLUMN cancelled_at DATETIME NULL AFTER created_at', 
    'SELECT "Column cancelled_at already exists" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add modified_at column if not exists
SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'modified_at');
SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE bookings ADD COLUMN modified_at DATETIME NULL AFTER cancelled_at', 
    'SELECT "Column modified_at already exists" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add checkin_actual_date column if not exists
SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'checkin_actual_date');
SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE bookings ADD COLUMN checkin_actual_date DATETIME NULL AFTER modified_at', 
    'SELECT "Column checkin_actual_date already exists" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add checkout_actual_date column if not exists
SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'checkout_actual_date');
SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE bookings ADD COLUMN checkout_actual_date DATETIME NULL AFTER checkin_actual_date', 
    'SELECT "Column checkout_actual_date already exists" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add discount_amount column if not exists
SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'discount_amount');
SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE bookings ADD COLUMN discount_amount DECIMAL(12,2) NOT NULL DEFAULT 0.00 AFTER payment_status', 
    'SELECT "Column discount_amount already exists" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add promotion_id column if not exists
SET @col_exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'promotion_id');
SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE bookings ADD COLUMN promotion_id BIGINT UNSIGNED NULL AFTER rateplan_snapshot_json', 
    'SELECT "Column promotion_id already exists" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add foreign key constraint for promotion_id if not exists
SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS 
    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND CONSTRAINT_NAME = 'fk_bookings_promotion');
SET @sql = IF(@fk_exists = 0 AND @col_exists = 1, 
    'ALTER TABLE bookings ADD CONSTRAINT fk_bookings_promotion FOREIGN KEY (promotion_id) REFERENCES promotions(id) ON UPDATE CASCADE ON DELETE SET NULL', 
    'SELECT "Foreign key fk_bookings_promotion already exists or promotion_id column missing" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verify changes
DESCRIBE bookings;

-- Show success message
SELECT 'Migration completed successfully! All missing columns added.' AS status;

ALTER TABLE housekeeping_tasks
ADD CONSTRAINT FK_housekeeping_tasks_bookings_booking_id
FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE SET NULL;


-- Add amount column to invoices table if it doesn't exist
ALTER TABLE `invoices` 
ADD COLUMN IF NOT EXISTS `amount` DECIMAL(12,2) NOT NULL DEFAULT 0.00 
AFTER `number`;

-- Kiểm tra và thêm cột amount nếu chưa tồn tại
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'invoices' 
    AND COLUMN_NAME = 'amount'
);

SET @query = IF(@col_exists = 0, 
    'ALTER TABLE `invoices` ADD COLUMN `amount` DECIMAL(12,2) NOT NULL DEFAULT 0.00 AFTER `number`;',
    'SELECT "Column amount already exists" AS message;'
);

PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
