-- Миграция: добавление группового чата компании
-- Применять вручную или через EF Core миграцию

-- 1. Таблица групповых чатов
CREATE TABLE IF NOT EXISTS CompanyChats (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL DEFAULT 'Чат компании',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Таблица участников чата
CREATE TABLE IF NOT EXISTS CompanyChatMembers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CompanyChatId INT NOT NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    CONSTRAINT FK_CompanyChatMembers_CompanyChats FOREIGN KEY (CompanyChatId) REFERENCES CompanyChats(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CompanyChatMembers_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_CompanyChatMembers_ChatUser UNIQUE (CompanyChatId, UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Добавляем CompanyChatId в ChatMessages и делаем TicketId nullable
ALTER TABLE ChatMessages
    MODIFY COLUMN TicketId INT NULL,
    ADD COLUMN CompanyChatId INT NULL,
    ADD CONSTRAINT FK_ChatMessages_CompanyChats FOREIGN KEY (CompanyChatId) REFERENCES CompanyChats(Id) ON DELETE CASCADE;

-- 4. Индекс для быстрого поиска сообщений по групповому чату
CREATE INDEX IX_ChatMessages_CompanyChatId ON ChatMessages(CompanyChatId);
