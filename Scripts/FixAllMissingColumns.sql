-- ============================================================
-- SCRIPT FIX ??Y ?? CHO C? 2 B?NG: INVOICES VÀ HOUSEKEEPING_TASKS
-- Ch?y toàn b? script này ?? fix t?t c? l?i missing columns
-- ============================================================

USE hotel_app;

-- ============================================================
-- PH?N 1: FIX B?NG INVOICES
-- ============================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS FixInvoicesTable$$

CREATE PROCEDURE FixInvoicesTable()
BEGIN
    DECLARE col_count INT;
    
    SELECT 'FIXING INVOICES TABLE...' AS status;
    
    -- Ki?m tra và thêm issued_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'issued_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `issued_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `amount`;
        SELECT 'Added: invoices.issued_at' AS result;
    ELSE
        SELECT 'Already exists: invoices.issued_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm status
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'status';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `status` VARCHAR(20) NOT NULL DEFAULT 'Draft' AFTER `issued_at`;
        SELECT 'Added: invoices.status' AS result;
    ELSE
        SELECT 'Already exists: invoices.status' AS result;
    END IF;
    
    -- Ki?m tra và thêm pdf_url
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'pdf_url';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `pdf_url` VARCHAR(255) NULL AFTER `status`;
        SELECT 'Added: invoices.pdf_url' AS result;
    ELSE
        SELECT 'Already exists: invoices.pdf_url' AS result;
    END IF;
    
    -- Ki?m tra và thêm notes
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'notes';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `notes` TEXT NULL AFTER `pdf_url`;
        SELECT 'Added: invoices.notes' AS result;
    ELSE
        SELECT 'Already exists: invoices.notes' AS result;
    END IF;
    
    -- Ki?m tra và thêm payment_method
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'payment_method';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `payment_method` VARCHAR(50) NULL AFTER `notes`;
        SELECT 'Added: invoices.payment_method' AS result;
    ELSE
        SELECT 'Already exists: invoices.payment_method' AS result;
    END IF;
    
    -- Ki?m tra và thêm paid_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'paid_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `paid_at` DATETIME NULL AFTER `payment_method`;
        SELECT 'Added: invoices.paid_at' AS result;
    ELSE
        SELECT 'Already exists: invoices.paid_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm created_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'created_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `paid_at`;
        SELECT 'Added: invoices.created_at' AS result;
    ELSE
        SELECT 'Already exists: invoices.created_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm updated_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'invoices'
    AND COLUMN_NAME = 'updated_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `invoices` ADD COLUMN `updated_at` DATETIME NULL AFTER `created_at`;
        SELECT 'Added: invoices.updated_at' AS result;
    ELSE
        SELECT 'Already exists: invoices.updated_at' AS result;
    END IF;
    
    SELECT 'INVOICES TABLE FIXED!' AS status;
    
END$$

DELIMITER ;

-- ============================================================
-- PH?N 2: FIX B?NG HOUSEKEEPING_TASKS
-- ============================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS FixHousekeepingTasksTable$$

CREATE PROCEDURE FixHousekeepingTasksTable()
BEGIN
    DECLARE col_count INT;
    
    SELECT 'FIXING HOUSEKEEPING_TASKS TABLE...' AS status;
    
    -- Ki?m tra và thêm task_type
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'task_type';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `task_type` VARCHAR(20) NOT NULL DEFAULT 'Cleaning' AFTER `assigned_to_user_id`;
        SELECT 'Added: housekeeping_tasks.task_type' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.task_type' AS result;
    END IF;
    
    -- Ki?m tra và thêm status
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'status';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `status` VARCHAR(20) NOT NULL DEFAULT 'Pending' AFTER `task_type`;
        SELECT 'Added: housekeeping_tasks.status' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.status' AS result;
    END IF;
    
    -- Ki?m tra và thêm priority
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'priority';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `priority` VARCHAR(20) NOT NULL DEFAULT 'Normal' AFTER `status`;
        SELECT 'Added: housekeeping_tasks.priority' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.priority' AS result;
    END IF;
    
    -- Ki?m tra và thêm scheduled_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'scheduled_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `scheduled_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `priority`;
        SELECT 'Added: housekeeping_tasks.scheduled_at' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.scheduled_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm completed_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'completed_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `completed_at` DATETIME NULL AFTER `scheduled_at`;
        SELECT 'Added: housekeeping_tasks.completed_at' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.completed_at' AS result;
    END IF;
    
    -- Ki?m tra và thêm notes
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'notes';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `notes` TEXT NULL AFTER `completed_at`;
        SELECT 'Added: housekeeping_tasks.notes' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.notes' AS result;
    END IF;
    
    -- Ki?m tra và thêm booking_id
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'booking_id';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `booking_id` BIGINT NULL AFTER `notes`;
        SELECT 'Added: housekeeping_tasks.booking_id' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.booking_id' AS result;
    END IF;
    
    -- Ki?m tra và thêm created_at
    SELECT COUNT(*) INTO col_count
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'hotel_app'
    AND TABLE_NAME = 'housekeeping_tasks'
    AND COLUMN_NAME = 'created_at';
    
    IF col_count = 0 THEN
        ALTER TABLE `housekeeping_tasks` ADD COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `booking_id`;
        SELECT 'Added: housekeeping_tasks.created_at' AS result;
    ELSE
        SELECT 'Already exists: housekeeping_tasks.created_at' AS result;
    END IF;
    
    SELECT 'HOUSEKEEPING_TASKS TABLE FIXED!' AS status;
    
END$$

DELIMITER ;

-- ============================================================
-- PH?N 3: TH?C THI CÁC PROCEDURE
-- ============================================================

SELECT 'STARTING DATABASE FIX...' AS message;

CALL FixInvoicesTable();

SELECT 'CONTINUING TO HOUSEKEEPING TASKS...' AS message;

CALL FixHousekeepingTasksTable();

SELECT 'DATABASE FIX COMPLETED!' AS message;

-- ============================================================
-- PH?N 4: KI?M TRA K?T QU?
-- ============================================================

SELECT 'FINAL STRUCTURE - INVOICES TABLE:' AS message;
SELECT 
    ORDINAL_POSITION AS 'Position',
    COLUMN_NAME AS 'Column',
    COLUMN_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'hotel_app'
AND TABLE_NAME = 'invoices'
ORDER BY ORDINAL_POSITION;

SELECT 'FINAL STRUCTURE - HOUSEKEEPING_TASKS TABLE:' AS message;
SELECT 
    ORDINAL_POSITION AS 'Position',
    COLUMN_NAME AS 'Column',
    COLUMN_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'hotel_app'
AND TABLE_NAME = 'housekeeping_tasks'
ORDER BY ORDINAL_POSITION;

-- ============================================================
-- PH?N 5: D?N D?P
-- ============================================================

DROP PROCEDURE IF EXISTS FixInvoicesTable;
DROP PROCEDURE IF EXISTS FixHousekeepingTasksTable;

SELECT 'ALL DONE! Please restart your .NET application.' AS final_message;
