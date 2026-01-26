CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `amenities` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `name` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_amenities` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `hotels` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `address` varchar(255) CHARACTER SET utf8mb4 NULL,
        `timezone` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_hotels` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `promotions` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `code` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
        `type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `value` decimal(65,30) NOT NULL,
        `start_date` datetime(6) NOT NULL,
        `end_date` datetime(6) NOT NULL,
        `conditions_json` longtext CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_promotions` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `users` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `username` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
        `password_hash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `role` longtext CHARACTER SET utf8mb4 NOT NULL,
        `email` varchar(128) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_users` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `room_types` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `hotel_id` bigint NOT NULL,
        `name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `capacity` tinyint unsigned NOT NULL,
        `base_price` decimal(65,30) NOT NULL,
        `description` longtext CHARACTER SET utf8mb4 NULL,
        `default_image_url` varchar(255) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_room_types` PRIMARY KEY (`id`),
        CONSTRAINT `FK_room_types_hotels_hotel_id` FOREIGN KEY (`hotel_id`) REFERENCES `hotels` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `guests` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `user_id` bigint NULL,
        `full_name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `email` varchar(128) CHARACTER SET utf8mb4 NULL,
        `phone` varchar(32) CHARACTER SET utf8mb4 NULL,
        `id_number` varchar(32) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_guests` PRIMARY KEY (`id`),
        CONSTRAINT `FK_guests_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `RefreshTokens` (
        `TokenId` int NOT NULL AUTO_INCREMENT,
        `UserId` bigint NOT NULL,
        `Token` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ExpiresAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`TokenId`),
        CONSTRAINT `FK_RefreshTokens_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `rateplans` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `room_type_id` bigint NOT NULL,
        `name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `free_cancel_until_hours` int NULL,
        `start_date` datetime(6) NOT NULL,
        `end_date` datetime(6) NOT NULL,
        `price` decimal(65,30) NOT NULL,
        `weekend_rule_json` longtext CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_rateplans` PRIMARY KEY (`id`),
        CONSTRAINT `FK_rateplans_room_types_room_type_id` FOREIGN KEY (`room_type_id`) REFERENCES `room_types` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `room_type_amenities` (
        `room_type_id` bigint NOT NULL,
        `amenity_id` bigint NOT NULL,
        CONSTRAINT `PK_room_type_amenities` PRIMARY KEY (`room_type_id`, `amenity_id`),
        CONSTRAINT `FK_room_type_amenities_amenities_amenity_id` FOREIGN KEY (`amenity_id`) REFERENCES `amenities` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_room_type_amenities_room_types_room_type_id` FOREIGN KEY (`room_type_id`) REFERENCES `room_types` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `rooms` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `hotel_id` bigint NOT NULL,
        `room_type_id` bigint NOT NULL,
        `number` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
        `floor` smallint NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `image_url` varchar(255) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_rooms` PRIMARY KEY (`id`),
        CONSTRAINT `FK_rooms_hotels_hotel_id` FOREIGN KEY (`hotel_id`) REFERENCES `hotels` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rooms_room_types_room_type_id` FOREIGN KEY (`room_type_id`) REFERENCES `room_types` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `bookings` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `hotel_id` bigint NOT NULL,
        `guest_id` bigint NOT NULL,
        `check_in_date` datetime(6) NOT NULL,
        `check_out_date` datetime(6) NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `total_amount` decimal(65,30) NOT NULL,
        `payment_status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `rateplan_snapshot_json` longtext CHARACTER SET utf8mb4 NULL,
        `promotion_id` bigint NULL,
        `discount_amount` decimal(65,30) NOT NULL,
        `cancelled_at` datetime(6) NULL,
        `modified_at` datetime(6) NULL,
        `checkin_actual_date` datetime(6) NULL,
        `checkout_actual_date` datetime(6) NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_bookings` PRIMARY KEY (`id`),
        CONSTRAINT `FK_bookings_guests_guest_id` FOREIGN KEY (`guest_id`) REFERENCES `guests` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_bookings_hotels_hotel_id` FOREIGN KEY (`hotel_id`) REFERENCES `hotels` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_bookings_promotions_promotion_id` FOREIGN KEY (`promotion_id`) REFERENCES `promotions` (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `booking_rooms` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `booking_id` bigint NOT NULL,
        `room_id` bigint NULL,
        `price_per_night` decimal(65,30) NOT NULL,
        `nights` int NOT NULL,
        CONSTRAINT `PK_booking_rooms` PRIMARY KEY (`id`),
        CONSTRAINT `FK_booking_rooms_bookings_booking_id` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE CASCADE,
        CONSTRAINT `FK_booking_rooms_rooms_room_id` FOREIGN KEY (`room_id`) REFERENCES `rooms` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `housekeeping_tasks` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `room_id` bigint NOT NULL,
        `assigned_to_user_id` bigint NULL,
        `task_type` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `priority` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `scheduled_at` datetime(6) NOT NULL,
        `completed_at` datetime(6) NULL,
        `notes` text CHARACTER SET utf8mb4 NULL,
        `booking_id` bigint NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_housekeeping_tasks` PRIMARY KEY (`id`),
        CONSTRAINT `FK_housekeeping_tasks_bookings_booking_id` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE SET NULL,
        CONSTRAINT `FK_housekeeping_tasks_rooms_room_id` FOREIGN KEY (`room_id`) REFERENCES `rooms` (`id`) ON DELETE CASCADE,
        CONSTRAINT `FK_housekeeping_tasks_users_assigned_to_user_id` FOREIGN KEY (`assigned_to_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `invoices` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `booking_id` bigint NOT NULL,
        `number` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
        `amount` decimal(12,2) NOT NULL,
        `issued_at` datetime(6) NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `pdf_url` varchar(255) CHARACTER SET utf8mb4 NULL,
        `notes` text CHARACTER SET utf8mb4 NULL,
        `payment_method` varchar(50) CHARACTER SET utf8mb4 NULL,
        `paid_at` datetime(6) NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NULL,
        CONSTRAINT `PK_invoices` PRIMARY KEY (`id`),
        CONSTRAINT `FK_invoices_bookings_booking_id` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE TABLE `payments` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `booking_id` bigint NOT NULL,
        `method` longtext CHARACTER SET utf8mb4 NOT NULL,
        `amount` decimal(65,30) NOT NULL,
        `txn_code` varchar(64) CHARACTER SET utf8mb4 NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_payments` PRIMARY KEY (`id`),
        CONSTRAINT `FK_payments_bookings_booking_id` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_booking_rooms_booking_id` ON `booking_rooms` (`booking_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_booking_rooms_room_id` ON `booking_rooms` (`room_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_bookings_guest_id` ON `bookings` (`guest_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_bookings_hotel_id` ON `bookings` (`hotel_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_bookings_promotion_id` ON `bookings` (`promotion_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_guests_user_id` ON `guests` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_housekeeping_tasks_assigned_to_user_id` ON `housekeeping_tasks` (`assigned_to_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_housekeeping_tasks_booking_id` ON `housekeeping_tasks` (`booking_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_housekeeping_tasks_room_id` ON `housekeeping_tasks` (`room_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_invoices_booking_id` ON `invoices` (`booking_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_payments_booking_id` ON `payments` (`booking_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_rateplans_room_type_id` ON `rateplans` (`room_type_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_RefreshTokens_UserId` ON `RefreshTokens` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_room_type_amenities_amenity_id` ON `room_type_amenities` (`amenity_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_room_types_hotel_id` ON `room_types` (`hotel_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_rooms_hotel_id` ON `rooms` (`hotel_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    CREATE INDEX `IX_rooms_room_type_id` ON `rooms` (`room_type_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260126084033_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260126084033_InitialCreate', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

