-- Migration: Update housekeeping_tasks table schema
-- Date: 2025-12-04
-- Description: Updates housekeeping_tasks table to match the HousekeepingTask entity model
-- Note: Run this script line by line if you encounter errors

-- Step 1: Add task_type column (ignore error if column exists)
ALTER TABLE housekeeping_tasks
ADD COLUMN task_type VARCHAR(20) NOT NULL DEFAULT 'Cleaning' AFTER assigned_to_user_id;

-- Step 2: Add priority column (ignore error if column exists)
ALTER TABLE housekeeping_tasks
ADD COLUMN priority VARCHAR(20) NOT NULL DEFAULT 'Normal' AFTER task_type;

-- Step 3: Add scheduled_at column (ignore error if column exists)
ALTER TABLE housekeeping_tasks
ADD COLUMN scheduled_at DATETIME NULL AFTER priority;

-- Step 4: Copy data from due_time to scheduled_at
UPDATE housekeeping_tasks 
SET scheduled_at = COALESCE(due_time, CURRENT_TIMESTAMP);

-- Step 5: Make scheduled_at NOT NULL after data is copied
ALTER TABLE housekeeping_tasks
MODIFY COLUMN scheduled_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;

-- Step 6: Add completed_at column (ignore error if column exists)
ALTER TABLE housekeeping_tasks
ADD COLUMN completed_at DATETIME NULL AFTER scheduled_at;

-- Step 7: Add booking_id column (ignore error if column exists)
ALTER TABLE housekeeping_tasks
ADD COLUMN booking_id BIGINT UNSIGNED NULL AFTER notes;

-- Step 8: Update status values to match entity model (Todo -> Pending, Done -> Completed, Blocked -> Cancelled)
-- Disable safe update mode temporarily
SET SQL_SAFE_UPDATES = 0;

UPDATE housekeeping_tasks 
SET status = CASE 
    WHEN status = 'Todo' THEN 'Pending'
    WHEN status = 'Done' THEN 'Completed'
    WHEN status = 'Blocked' THEN 'Cancelled'
    ELSE status
END;

SET SQL_SAFE_UPDATES = 1;

-- Step 9: Modify status column to use VARCHAR instead of ENUM
ALTER TABLE housekeeping_tasks 
MODIFY COLUMN status VARCHAR(20) NOT NULL DEFAULT 'Pending';
