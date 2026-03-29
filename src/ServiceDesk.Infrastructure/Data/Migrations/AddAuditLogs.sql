-- Миграция: добавление таблицы журнала аудита
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Action INT NOT NULL COMMENT '0=Created,1=Updated,2=Deleted,3=StatusChanged,4=Assigned,5=Login,6=Logout,7=PartsApproved,8=PartsRejected,9=ConsentGranted,10=ConsentRevoked,11=PersonalDataRequested,12=PersonalDataExported,13=PersonalDataDeleted',
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
