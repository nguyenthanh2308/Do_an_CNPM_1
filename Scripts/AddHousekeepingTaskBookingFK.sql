-- Add foreign key constraint between housekeeping_tasks and bookings
-- This script should be run manually on the database

USE hotel_app;

-- Check if the foreign key already exists
SELECT 
    CONSTRAINT_NAME 
FROM 
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
WHERE 
    TABLE_SCHEMA = 'hotel_app' 
    AND TABLE_NAME = 'housekeeping_tasks' 
    AND CONSTRAINT_NAME = 'FK_housekeeping_tasks_bookings_booking_id';

-- If the foreign key doesn't exist, add it
-- First, make sure all existing booking_id values are valid (NULL or exist in bookings table)
-- Update any invalid booking_id values to NULL
UPDATE housekeeping_tasks 
SET booking_id = NULL 
WHERE booking_id IS NOT NULL 
  AND booking_id NOT IN (SELECT id FROM bookings);

-- Now add the foreign key constraint
ALTER TABLE housekeeping_tasks
ADD CONSTRAINT FK_housekeeping_tasks_bookings_booking_id
FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE SET NULL;

-- Verify the constraint was added
SELECT 
    CONSTRAINT_NAME,
    COLUMN_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM 
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
WHERE 
    TABLE_SCHEMA = 'hotel_app' 
    AND TABLE_NAME = 'housekeeping_tasks'
    AND CONSTRAINT_NAME = 'FK_housekeeping_tasks_bookings_booking_id';

-- Redirect theo role - tất cả đều về Home/Index để xem dashboard
return RedirectToAction("Index", "Home");
