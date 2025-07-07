CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `UserBPFiles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `FileName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OriginalFilePath` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FromatFilePath` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreateTime` datetime(6) NOT NULL,
    CONSTRAINT `PK_UserBPFiles` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Phone` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Avatar` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Gender` tinyint unsigned NOT NULL,
    `Address` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Company` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Tel` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Province` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ProvinceId` int NOT NULL,
    `City` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CityId` int NOT NULL,
    `NameCard` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserTags` (
    `UserId` int NOT NULL,
    `Tag` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `CreateTime` datetime(6) NOT NULL,
    CONSTRAINT `PK_UserTags` PRIMARY KEY (`UserId`, `Tag`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserProperties` (
    `AppUserId` int NOT NULL,
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Text` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_UserProperties` PRIMARY KEY (`AppUserId`, `Key`, `Value`),
    CONSTRAINT `FK_UserProperties_Users_AppUserId` FOREIGN KEY (`AppUserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250627230028_initDB', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` ADD `Age` longtext CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250627234112_InsertAge', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` MODIFY COLUMN `Age` longtext CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250627235145_UpdateAgeColumn', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` MODIFY COLUMN `Age` int NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250627235404_updatecolumnType', '8.0.0');

COMMIT;

