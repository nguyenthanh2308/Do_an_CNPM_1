-- Script an toàn ?? thêm các c?t cho b?ng invoices
-- Script này s? KHÔNG gây l?i n?u c?t ?ã t?n t?i

USE hotel_app;

-- T?o procedure t?m th?i
DELIMITER $$

DROP PROCEDURE IF EXISTS AddInvoiceColumnsIfNotExists$$

CREATE PROCEDURE AddInvoiceColumnsIfNotExists()
BEGIN
    DECLARE col_count INT;
    
    -- Ki?m tra và thêm issued_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'issued_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `issued_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `amount`;
        SELECT 'Added: issued_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm status
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'status';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `status` VARCHAR(20) NOT NULL DEFAULT 'Draft' AFTER `issued_at`;
        SELECT 'Added: status' AS result;
    END IF;
    
    -- Ki?m tra và thêm pdf_url
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'pdf_url';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `pdf_url` VARCHAR(255) NULL AFTER `status`;
        SELECT 'Added: pdf_url' AS result;
    END IF;
    
    -- Ki?m tra và thêm notes
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'notes';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `notes` TEXT NULL AFTER `pdf_url`;
        SELECT 'Added: notes' AS result;
    END IF;
    
    -- Ki?m tra và thêm payment_method
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'payment_method';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `payment_method` VARCHAR(50) NULL AFTER `notes`;
        SELECT 'Added: payment_method' AS result;
    END IF;
    
    -- Ki?m tra và thêm paid_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'paid_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `paid_at` DATETIME NULL AFTER `payment_method`;
        SELECT 'Added: paid_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm created_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'created_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `paid_at`;
        SELECT 'Added: created_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm updated_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'updated_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `updated_at` DATETIME NULL AFTER `created_at`;
        SELECT 'Added: updated_at' AS result;
    END IF;
    
END$$

DELIMITER ;

-- G?i procedure
CALL AddInvoiceColumnsIfNotExists();

-- Hi?n th? c?u trúc cu?i cùng
SELECT 
    ORDINAL_POSITION,
    COLUMN_NAME, 
    COLUMN_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'hotel_app'
AND TABLE_NAME = 'invoices'
ORDER BY ORDINAL_POSITION;

-- Xóa procedure
DROP PROCEDURE IF EXISTS AddInvoiceColumnsIfNotExists;
