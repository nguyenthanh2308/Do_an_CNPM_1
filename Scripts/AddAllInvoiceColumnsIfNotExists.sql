-- Script an toàn ?? thêm các c?t còn thi?u vào b?ng invoices
-- Script này ki?m tra tr??c khi thêm, không gây l?i n?u c?t ?ã t?n t?i

DELIMITER $$

DROP PROCEDURE IF EXISTS AddInvoiceColumns$$

CREATE PROCEDURE AddInvoiceColumns()
BEGIN
    DECLARE col_exists INT;
    
    -- 1. Ki?m tra và thêm c?t issued_at
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'issued_at';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `issued_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        AFTER `amount`;
        SELECT 'Added column: issued_at' AS message;
    ELSE
        SELECT 'Column issued_at already exists' AS message;
    END IF;
    
    -- 2. Ki?m tra và thêm c?t status
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'status';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `status` VARCHAR(20) NOT NULL DEFAULT 'Draft'
        AFTER `issued_at`;
        SELECT 'Added column: status' AS message;
    ELSE
        SELECT 'Column status already exists' AS message;
    END IF;
    
    -- 3. Ki?m tra và thêm c?t pdf_url
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'pdf_url';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `pdf_url` VARCHAR(255) NULL
        AFTER `status`;
        SELECT 'Added column: pdf_url' AS message;
    ELSE
        SELECT 'Column pdf_url already exists' AS message;
    END IF;
    
    -- 4. Ki?m tra và thêm c?t notes
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'notes';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `notes` TEXT NULL
        AFTER `pdf_url`;
        SELECT 'Added column: notes' AS message;
    ELSE
        SELECT 'Column notes already exists' AS message;
    END IF;
    
    -- 5. Ki?m tra và thêm c?t payment_method
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'payment_method';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `payment_method` VARCHAR(50) NULL
        AFTER `notes`;
        SELECT 'Added column: payment_method' AS message;
    ELSE
        SELECT 'Column payment_method already exists' AS message;
    END IF;
    
    -- 6. Ki?m tra và thêm c?t paid_at
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'paid_at';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `paid_at` DATETIME NULL
        AFTER `payment_method`;
        SELECT 'Added column: paid_at' AS message;
    ELSE
        SELECT 'Column paid_at already exists' AS message;
    END IF;
    
    -- 7. Ki?m tra và thêm c?t created_at
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'created_at';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        AFTER `paid_at`;
        SELECT 'Added column: created_at' AS message;
    ELSE
        SELECT 'Column created_at already exists' AS message;
    END IF;
    
    -- 8. Ki?m tra và thêm c?t updated_at
    SELECT COUNT(*) INTO col_exists
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'updated_at';
    
    IF col_exists = 0 THEN
        ALTER TABLE `invoices` 
        ADD COLUMN `updated_at` DATETIME NULL
        AFTER `created_at`;
        SELECT 'Added column: updated_at' AS message;
    ELSE
        SELECT 'Column updated_at already exists' AS message;
    END IF;
    
    -- Hi?n th? c?u trúc b?ng sau khi c?p nh?t
    SELECT 'Invoice table structure updated successfully!' AS final_message;
    
END$$

DELIMITER ;

-- G?i procedure
CALL AddInvoiceColumns();

-- Ki?m tra k?t qu?
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT,
    COLUMN_TYPE,
    ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'invoices'
ORDER BY ORDINAL_POSITION;

-- Xóa procedure sau khi dùng xong
DROP PROCEDURE IF EXISTS AddInvoiceColumns;
