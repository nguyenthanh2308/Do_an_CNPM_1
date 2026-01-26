USE hotel_app;

CREATE TABLE IF NOT EXISTS `RefreshTokens` (
    `TokenId` int NOT NULL AUTO_INCREMENT,
    `UserId` bigint NOT NULL,
    `Token` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`TokenId`),
    KEY `IX_RefreshTokens_UserId` (`UserId`),
    CONSTRAINT `FK_RefreshTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Verify table created
SHOW TABLES LIKE 'RefreshTokens';
DESCRIBE RefreshTokens;
