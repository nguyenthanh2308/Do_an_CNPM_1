-- Script ?? thêm các c?t còn thi?u vào b?ng invoices
-- Ch?y t?ng câu l?nh m?t, n?u c?t ?ã t?n t?i thì b? qua l?i

-- 1. Thêm c?t issued_at (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `issued_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
AFTER `amount`;

-- 2. Thêm c?t status (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `status` VARCHAR(20) NOT NULL DEFAULT 'Draft'
AFTER `issued_at`;

-- 3. Thêm c?t pdf_url (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `pdf_url` VARCHAR(255) NULL
AFTER `status`;

-- 4. Thêm c?t notes (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `notes` TEXT NULL
AFTER `pdf_url`;

-- 5. Thêm c?t payment_method (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `payment_method` VARCHAR(50) NULL
AFTER `notes`;

-- 6. Thêm c?t paid_at (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `paid_at` DATETIME NULL
AFTER `payment_method`;

-- 7. Thêm c?t created_at (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
AFTER `paid_at`;

-- 8. Thêm c?t updated_at (n?u ch?a có)
ALTER TABLE `invoices` 
ADD COLUMN `updated_at` DATETIME NULL
AFTER `created_at`;

-- Ki?m tra k?t qu?
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT,
    COLUMN_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'invoices'
ORDER BY ORDINAL_POSITION;
