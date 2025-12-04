-- Update housekeeping_tasks table to match entity model
-- This script updates the table schema to match HousekeepingTask.cs
use hotel_app;
-- Step 1: Add new columns if they don't exist
ALTER TABLE housekeeping_tasks
ADD COLUMN IF NOT EXISTS task_type VARCHAR(20) NOT NULL DEFAULT 'Cleaning' AFTER assigned_to_user_id,
ADD COLUMN IF NOT EXISTS priority VARCHAR(20) NOT NULL DEFAULT 'Normal' AFTER task_type,
ADD COLUMN IF NOT EXISTS scheduled_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER priority,
ADD COLUMN IF NOT EXISTS completed_at DATETIME NULL AFTER scheduled_at,
ADD COLUMN IF NOT EXISTS booking_id BIGINT UNSIGNED NULL AFTER notes;

-- Step 2: Copy data from due_time to scheduled_at if scheduled_at was just created
UPDATE housekeeping_tasks 
SET scheduled_at = COALESCE(due_time, CURRENT_TIMESTAMP)
WHERE scheduled_at IS NULL OR scheduled_at = '0000-00-00 00:00:00';

-- Step 3: Update status values to match entity model
-- Old: 'Todo','InProgress','Done','Blocked'
-- New: 'Pending','InProgress','Completed','Cancelled'
UPDATE housekeeping_tasks 
SET status = CASE 
    WHEN status = 'Todo' THEN 'Pending'
    WHEN status = 'Done' THEN 'Completed'
    WHEN status = 'Blocked' THEN 'Cancelled'
    ELSE status
END;

-- Step 4: Modify status column to use VARCHAR instead of ENUM
ALTER TABLE housekeeping_tasks 
MODIFY COLUMN status VARCHAR(20) NOT NULL DEFAULT 'Pending';

-- Step 5: Add foreign key for booking_id if it doesn't exist
SET @constraint_exists = (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'housekeeping_tasks' 
    AND CONSTRAINT_NAME = 'FK_housekeeping_tasks_bookings_booking_id');

SET @sql = IF(@constraint_exists = 0,
    'ALTER TABLE housekeeping_tasks ADD CONSTRAINT FK_housekeeping_tasks_bookings_booking_id FOREIGN KEY (booking_id) REFERENCES bookings(id) ON UPDATE CASCADE ON DELETE SET NULL',
    'SELECT "Foreign key already exists" AS message');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Step 6: Drop due_time column (optional - only after confirming data is migrated)
-- Uncomment the following line after verifying the migration is successful
-- ALTER TABLE housekeeping_tasks DROP COLUMN IF EXISTS due_time;

-- Verify the changes
DESCRIBE housekeeping_tasks;
