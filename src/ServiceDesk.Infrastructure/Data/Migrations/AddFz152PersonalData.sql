-- Миграция: ФЗ-152 — Согласия на обработку ПД и запросы субъектов ПД
-- Дата: 2026-03-22

-- Добавляем поле согласия на обработку ПД в таблицу пользователей
ALTER TABLE Users ADD COLUMN PersonalDataConsentAt DATETIME(6) NULL;

-- Таблица согласий (ФЗ-152, ст. 9)
CREATE TABLE IF NOT EXISTS UserConsents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    ConsentType INT NOT NULL COMMENT '0=ОбработкаПД, 1=SMS, 2=Push, 3=ТретьиЛица',
    ConsentVersion VARCHAR(20) NOT NULL,
    ConsentText TEXT NOT NULL,
    IsGranted TINYINT(1) NOT NULL DEFAULT 1,
    GrantedAt DATETIME(6) NOT NULL,
    RevokedAt DATETIME(6) NULL,
    IpAddress VARCHAR(45) NULL,
    UserAgent VARCHAR(500) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX IX_UserConsents_UserId_ConsentType (UserId, ConsentType),
    CONSTRAINT FK_UserConsents_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Таблица запросов субъектов ПД (ФЗ-152, ст. 14–17)
CREATE TABLE IF NOT EXISTS PersonalDataRequests (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    RequestType INT NOT NULL COMMENT '0=Доступ, 1=Исправление, 2=Удаление, 3=ОтзывСогласия',
    Status INT NOT NULL DEFAULT 0 COMMENT '0=Новый, 1=ВОбработке, 2=Выполнен, 3=Отклонён',
    Description TEXT NOT NULL,
    Response TEXT NULL,
    ProcessedByUserId INT NULL,
    ProcessedAt DATETIME(6) NULL,
    Deadline DATETIME(6) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX IX_PersonalDataRequests_UserId (UserId),
    INDEX IX_PersonalDataRequests_Status (Status),
    CONSTRAINT FK_PersonalDataRequests_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PersonalDataRequests_Users_ProcessedByUserId FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
