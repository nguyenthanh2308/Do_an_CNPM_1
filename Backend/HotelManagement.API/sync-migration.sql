-- Tạo bảng __EFMigrationsHistory nếu chưa có
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

-- Thêm migration hiện tại vào history (đánh dấu là đã áp dụng)
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260126084033_InitialCreate', '8.0.0')
ON DUPLICATE KEY UPDATE `ProductVersion` = '8.0.0';
