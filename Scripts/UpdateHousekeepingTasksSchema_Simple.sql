-- Simple Update housekeeping_tasks table to match entity model
-- Run this script in your MySQL client (Workbench, phpMyAdmin, etc.)

-- Step 1: Add new columns
ALTER TABLE housekeeping_tasks
ADD COLUMN task_type VARCHAR(20) NOT NULL DEFAULT 'Cleaning' AFTER assigned_to_user_id;

ALTER TABLE housekeeping_tasks
ADD COLUMN priority VARCHAR(20) NOT NULL DEFAULT 'Normal' AFTER task_type;

ALTER TABLE housekeeping_tasks
ADD COLUMN scheduled_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER priority;

ALTER TABLE housekeeping_tasks
ADD COLUMN completed_at DATETIME NULL AFTER scheduled_at;

ALTER TABLE housekeeping_tasks
ADD COLUMN booking_id BIGINT UNSIGNED NULL AFTER notes;

-- Step 2: Copy data from due_time to scheduled_at
UPDATE housekeeping_tasks 
SET scheduled_at = COALESCE(due_time, CURRENT_TIMESTAMP);

-- Step 3: Update status values to match entity model
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

-- Step 5: Add foreign key for booking_id
ALTER TABLE housekeeping_tasks 
ADD CONSTRAINT FK_housekeeping_tasks_bookings_booking_id 
FOREIGN KEY (booking_id) REFERENCES bookings(id) 
ON UPDATE CASCADE ON DELETE SET NULL;

-- Step 6: Verify the changes
DESCRIBE housekeeping_tasks;
SELECT * FROM housekeeping_tasks LIMIT 5;
