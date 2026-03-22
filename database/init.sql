-- ============================================================
-- ServiceDesk — База данных timewisev2
-- Создание таблиц и начальные данные
-- MySQL 8.x
-- ============================================================

CREATE DATABASE IF NOT EXISTS timewisev2
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE timewisev2;

-- ============================================================
-- 1. Клиенты (организации)
-- ============================================================
CREATE TABLE IF NOT EXISTS Clients (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(300) NOT NULL COMMENT 'Наименование организации',
    Inn VARCHAR(12) NULL COMMENT 'ИНН',
    Network VARCHAR(200) NULL COMMENT 'Сеть',
    Phone VARCHAR(20) NULL COMMENT 'Контактный телефон',
    Email VARCHAR(255) NULL COMMENT 'Email',
    LegalAddress VARCHAR(500) NULL COMMENT 'Юридический адрес',
    IsActive TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Активен ли клиент',
    TtkFilePath VARCHAR(500) NULL COMMENT 'Путь к файлу ТТК',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_clients_name (Name),
    INDEX idx_clients_network (Network)
) ENGINE=InnoDB COMMENT='Клиенты (организации)';

-- ============================================================
-- 2. Пользователи системы
-- ============================================================
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LastName VARCHAR(100) NOT NULL COMMENT 'Фамилия',
    FirstName VARCHAR(100) NOT NULL COMMENT 'Имя',
    MiddleName VARCHAR(100) NULL COMMENT 'Отчество',
    Phone VARCHAR(20) NOT NULL COMMENT 'Номер телефона (логин)',
    Email VARCHAR(255) NULL COMMENT 'Email',
    Role INT NOT NULL DEFAULT 5 COMMENT '0=Техник,1=Инженер,2=ГлавИнженер,3=Логист,4=МенеджерTW,5=Клиент,6=МенеджерКлиента,7=Модератор',
    Company VARCHAR(255) NULL COMMENT 'Компания',
    IsActive TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Активен ли аккаунт',
    LastLoginAt DATETIME(6) NULL COMMENT 'Последний вход',
    SmsCode VARCHAR(10) NULL COMMENT 'SMS-код',
    SmsCodeSentAt DATETIME(6) NULL COMMENT 'Время отправки SMS',
    SmsAttempts INT NOT NULL DEFAULT 0 COMMENT 'Попытки ввода кода',
    SmsBlockedUntil DATETIME(6) NULL COMMENT 'Блокировка SMS до',
    ClientId INT NULL COMMENT 'Привязка к клиенту (для менеджера клиента)',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    UNIQUE INDEX idx_users_phone (Phone),
    INDEX idx_users_role (Role),
    CONSTRAINT fk_users_client FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Пользователи системы';

-- ============================================================
-- 3. Контактные лица клиентов
-- ============================================================
CREATE TABLE IF NOT EXISTS ContactPersons (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(300) NOT NULL COMMENT 'ФИО',
    Phone VARCHAR(20) NOT NULL COMMENT 'Телефон',
    Email VARCHAR(255) NULL COMMENT 'Email',
    Position VARCHAR(200) NULL COMMENT 'Должность',
    ClientId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    CONSTRAINT fk_contacts_client FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Контактные лица клиентов';

-- ============================================================
-- 4. Точки обслуживания
-- ============================================================
CREATE TABLE IF NOT EXISTS ServicePoints (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL COMMENT 'Название точки',
    Address VARCHAR(500) NOT NULL COMMENT 'Адрес',
    Region VARCHAR(100) NULL COMMENT 'Регион',
    Network VARCHAR(200) NULL COMMENT 'Сеть клиента',
    Latitude DOUBLE NULL COMMENT 'Широта',
    Longitude DOUBLE NULL COMMENT 'Долгота',
    ContactPhone VARCHAR(20) NULL COMMENT 'Контактный телефон',
    ContactPerson VARCHAR(200) NULL COMMENT 'Контактное лицо',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    ClientId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_sp_region (Region),
    INDEX idx_sp_network (Network),
    CONSTRAINT fk_sp_client FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE RESTRICT
) ENGINE=InnoDB COMMENT='Точки обслуживания';

-- ============================================================
-- 5. Оборудование
-- ============================================================
CREATE TABLE IF NOT EXISTS Equipment (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Model VARCHAR(200) NOT NULL COMMENT 'Модель оборудования',
    SerialNumber VARCHAR(100) NOT NULL COMMENT 'Серийный номер',
    InstalledAt DATETIME(6) NULL COMMENT 'Дата установки',
    Description VARCHAR(1000) NULL COMMENT 'Описание',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    ServicePointId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    UNIQUE INDEX idx_eq_serial (SerialNumber),
    CONSTRAINT fk_eq_sp FOREIGN KEY (ServicePointId) REFERENCES ServicePoints(Id) ON DELETE RESTRICT
) ENGINE=InnoDB COMMENT='Оборудование';

-- ============================================================
-- 6. Заявки
-- ============================================================
CREATE TABLE IF NOT EXISTS Tickets (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TicketNumber VARCHAR(20) NOT NULL COMMENT 'Номер заявки',
    Type INT NOT NULL DEFAULT 0 COMMENT '0=Ремонт,1=ТО,2=Установка,3=Демонтаж,4=Поставка,5=Консультация',
    Status INT NOT NULL DEFAULT 0 COMMENT '0=Новая,...,10=Отменена',
    Priority INT NOT NULL DEFAULT 1 COMMENT '0=Низкий,1=Обычный,2=Высокий,3=Критичный',
    Description VARCHAR(2000) NOT NULL COMMENT 'Описание проблемы',
    Comment VARCHAR(1000) NULL COMMENT 'Комментарий',
    Deadline DATETIME(6) NULL COMMENT 'Дедлайн',
    WorkStartedAt DATETIME(6) NULL COMMENT 'Начало работ',
    WorkCompletedAt DATETIME(6) NULL COMMENT 'Завершение работ',
    AssignedEngineerId INT NULL COMMENT 'Назначенный специалист',
    ServicePointId INT NOT NULL COMMENT 'Точка обслуживания',
    EquipmentId INT NULL COMMENT 'Оборудование',
    CreatedByUserId INT NOT NULL COMMENT 'Создатель заявки',
    AvrPhotoPath VARCHAR(500) NULL COMMENT 'Фото АВР',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    UNIQUE INDEX idx_tickets_number (TicketNumber),
    INDEX idx_tickets_status (Status),
    INDEX idx_tickets_created (CreatedAt),
    INDEX idx_tickets_engineer (AssignedEngineerId),
    INDEX idx_tickets_sp (ServicePointId),
    CONSTRAINT fk_tickets_engineer FOREIGN KEY (AssignedEngineerId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT fk_tickets_sp FOREIGN KEY (ServicePointId) REFERENCES ServicePoints(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_tickets_eq FOREIGN KEY (EquipmentId) REFERENCES Equipment(Id) ON DELETE SET NULL,
    CONSTRAINT fk_tickets_creator FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB COMMENT='Заявки на обслуживание';

-- ============================================================
-- 6а. Вложения заявок (фото/видео при создании)
-- ============================================================
CREATE TABLE IF NOT EXISTS TicketAttachments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FileName VARCHAR(500) NOT NULL COMMENT 'Имя файла',
    FilePath VARCHAR(500) NOT NULL COMMENT 'Путь на сервере',
    ContentType VARCHAR(100) NOT NULL COMMENT 'MIME-тип',
    FileSize BIGINT NOT NULL DEFAULT 0 COMMENT 'Размер в байтах',
    TicketId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_ta_ticket (TicketId),
    CONSTRAINT fk_ta_ticket FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Вложения заявок (фото/видео)';

-- ============================================================
-- 7. Запчасти (модуль неактивен, таблицы подготовлены)
-- ============================================================
CREATE TABLE IF NOT EXISTS SpareParts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Article VARCHAR(50) NOT NULL COMMENT 'Артикул',
    Name VARCHAR(300) NOT NULL COMMENT 'Название',
    Description VARCHAR(1000) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0 COMMENT 'Текущая цена',
    IsExpensive TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Дорогостоящая (требует согласования)',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    UNIQUE INDEX idx_sp_article (Article)
) ENGINE=InnoDB COMMENT='Справочник запчастей';

CREATE TABLE IF NOT EXISTS SparePartPriceHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SparePartId INT NOT NULL,
    OldPrice DECIMAL(18,2) NOT NULL,
    NewPrice DECIMAL(18,2) NOT NULL,
    ChangedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    CONSTRAINT fk_sph_sp FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='История цен запчастей';

CREATE TABLE IF NOT EXISTS SparePartStocks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SparePartId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 0 COMMENT 'Количество',
    Location VARCHAR(200) NOT NULL DEFAULT '' COMMENT 'Местоположение',
    EngineerId INT NULL COMMENT 'У инженера (null=склад)',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    CONSTRAINT fk_sps_sp FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id) ON DELETE CASCADE,
    CONSTRAINT fk_sps_eng FOREIGN KEY (EngineerId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Остатки запчастей';

-- ============================================================
-- 8. Связь заявка-запчасть (многие-ко-многим)
-- ============================================================
CREATE TABLE IF NOT EXISTS TicketSpareParts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TicketId INT NOT NULL,
    SparePartId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    PriceAtTime DECIMAL(18,2) NOT NULL DEFAULT 0 COMMENT 'Цена на момент добавления',
    IsApproved TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Согласована',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_tsp_ticket (TicketId),
    CONSTRAINT fk_tsp_ticket FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE,
    CONSTRAINT fk_tsp_sp FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Запчасти в заявке';

-- ============================================================
-- 9. Чат заявки
-- ============================================================
CREATE TABLE IF NOT EXISTS ChatMessages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Text VARCHAR(4000) NOT NULL COMMENT 'Текст сообщения',
    TicketId INT NOT NULL,
    SenderId INT NOT NULL,
    IsRead TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_chat_ticket (TicketId),
    CONSTRAINT fk_chat_ticket FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE,
    CONSTRAINT fk_chat_sender FOREIGN KEY (SenderId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB COMMENT='Сообщения чата заявки';

CREATE TABLE IF NOT EXISTS ChatAttachments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FileName VARCHAR(500) NOT NULL COMMENT 'Имя файла',
    FilePath VARCHAR(500) NOT NULL COMMENT 'Путь на сервере',
    ContentType VARCHAR(100) NOT NULL COMMENT 'MIME-тип',
    FileSize BIGINT NOT NULL DEFAULT 0 COMMENT 'Размер в байтах',
    ChatMessageId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    CONSTRAINT fk_attach_msg FOREIGN KEY (ChatMessageId) REFERENCES ChatMessages(Id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Вложения чата';

-- ============================================================
-- 10. Уведомления
-- ============================================================
CREATE TABLE IF NOT EXISTS Notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(300) NOT NULL COMMENT 'Заголовок',
    Message VARCHAR(1000) NOT NULL COMMENT 'Текст',
    Url VARCHAR(500) NULL COMMENT 'Ссылка',
    IsRead TINYINT(1) NOT NULL DEFAULT 0,
    UserId INT NOT NULL COMMENT 'Получатель',
    TicketId INT NULL COMMENT 'Связанная заявка',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_notif_user (UserId),
    INDEX idx_notif_read (UserId, IsRead),
    CONSTRAINT fk_notif_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT fk_notif_ticket FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Уведомления пользователей';

-- ============================================================
-- 11. Журнал аудита
-- ============================================================
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Action INT NOT NULL COMMENT '0=Created,1=Updated,2=Deleted,3=StatusChanged,4=Assigned,5=Login,6=Logout,7=PartsApproved,8=PartsRejected',
    EntityType VARCHAR(100) NOT NULL COMMENT 'Тип сущности',
    EntityId INT NOT NULL COMMENT 'ID сущности',
    OldValue VARCHAR(1000) NULL COMMENT 'Старое значение',
    NewValue VARCHAR(1000) NULL COMMENT 'Новое значение',
    UserId INT NOT NULL COMMENT 'Кто выполнил',
    IpAddress VARCHAR(45) NULL COMMENT 'IP-адрес',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_audit_entity (EntityType, EntityId),
    INDEX idx_audit_created (CreatedAt),
    INDEX idx_audit_user (UserId),
    CONSTRAINT fk_audit_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB COMMENT='Журнал аудита действий';

-- ============================================================
-- 16. Согласия пользователей (ФЗ-152, ст. 9)
-- ============================================================
CREATE TABLE IF NOT EXISTS UserConsents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL COMMENT 'Пользователь, давший согласие',
    ConsentType TINYINT NOT NULL DEFAULT 0 COMMENT '0=ОбработкаПД, 1=SMS, 2=Push, 3=ПередачаТретьимЛицам',
    ConsentVersion VARCHAR(50) NOT NULL COMMENT 'Версия текста согласия',
    ConsentText TEXT NOT NULL COMMENT 'Текст согласия на момент подписания',
    IsGranted TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Согласие дано',
    GrantedAt DATETIME(6) NOT NULL COMMENT 'Дата предоставления согласия',
    RevokedAt DATETIME(6) NULL COMMENT 'Дата отзыва согласия (NULL — действует)',
    IpAddress VARCHAR(45) NULL COMMENT 'IP-адрес при подписании',
    UserAgent VARCHAR(500) NULL COMMENT 'User-Agent при подписании',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_consents_user (UserId),
    INDEX idx_consents_type (ConsentType),
    INDEX idx_consents_granted (GrantedAt),
    CONSTRAINT fk_consents_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB COMMENT='Согласия пользователей на обработку ПД (ФЗ-152)';

-- ============================================================
-- 17. Запросы субъектов ПД (ФЗ-152, ст. 14–17)
-- ============================================================
CREATE TABLE IF NOT EXISTS PersonalDataRequests (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL COMMENT 'Пользователь-заявитель',
    RequestType TINYINT NOT NULL DEFAULT 0 COMMENT '0=Доступ, 1=Исправление, 2=Удаление, 3=ОтзывСогласия',
    Status TINYINT NOT NULL DEFAULT 0 COMMENT '0=Новый, 1=ВОбработке, 2=Выполнен, 3=Отклонён',
    Description TEXT NOT NULL COMMENT 'Описание запроса от пользователя',
    Response TEXT NULL COMMENT 'Ответ оператора',
    ProcessedByUserId INT NULL COMMENT 'Кто обработал запрос (модератор)',
    ProcessedAt DATETIME(6) NULL COMMENT 'Дата обработки',
    Deadline DATETIME(6) NOT NULL COMMENT 'Крайний срок обработки (30 дней по ФЗ-152)',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    INDEX idx_pdr_user (UserId),
    INDEX idx_pdr_status (Status),
    INDEX idx_pdr_deadline (Deadline),
    INDEX idx_pdr_created (CreatedAt),
    CONSTRAINT fk_pdr_user FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT fk_pdr_processed_by FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB COMMENT='Запросы субъектов ПД на доступ/исправление/удаление (ФЗ-152)';

-- ============================================================
-- НАЧАЛЬНЫЕ ДАННЫЕ
-- ============================================================

-- Модератор (администратор системы)
INSERT INTO Users (LastName, FirstName, Phone, Role, IsActive, CreatedAt)
VALUES ('Администратор', 'Системный', '+70000000000', 7, 1, NOW(6));

-- Тестовый клиент
INSERT INTO Clients (Name, Inn, Network, Phone, Email, CreatedAt)
VALUES ('ООО «Тестовая компания»', '1234567890', 'Тестовая сеть', '+71111111111', 'test@example.com', NOW(6));

-- Тестовая точка обслуживания
INSERT INTO ServicePoints (Name, Address, Region, Network, Latitude, Longitude, ClientId, CreatedAt)
VALUES ('Офис на Ленина', 'г. Москва, ул. Ленина, д. 1', 'Москва', 'Тестовая сеть', 55.7558, 37.6173, 1, NOW(6));

-- Тестовое оборудование
INSERT INTO Equipment (Model, SerialNumber, InstalledAt, ServicePointId, Description, CreatedAt)
VALUES ('DeLonghi ECAM 370.95.T', 'SN-2024-001', DATE_SUB(NOW(), INTERVAL 6 MONTH), 1, 'Кофемашина автоматическая', NOW(6));

-- Тестовый инженер
INSERT INTO Users (LastName, FirstName, MiddleName, Phone, Role, IsActive, CreatedAt)
VALUES ('Иванов', 'Пётр', 'Сергеевич', '+72222222222', 1, 1, NOW(6));

-- Тестовый логист
INSERT INTO Users (LastName, FirstName, Phone, Role, IsActive, CreatedAt)
VALUES ('Петрова', 'Анна', '+73333333333', 3, 1, NOW(6));

-- Тестовая заявка
INSERT INTO Tickets (TicketNumber, Type, Status, Priority, Description, ServicePointId, EquipmentId, CreatedByUserId, CreatedAt)
VALUES ('SD-202603-0001', 0, 0, 1, 'Кофемашина не включается, индикатор мигает красным', 1, 1, 1, NOW(6));

SELECT 'База данных timewisev2 успешно создана и заполнена!' AS result;
