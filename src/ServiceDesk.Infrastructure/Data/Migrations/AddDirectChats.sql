-- Миграция: добавление поддержки личных чатов между сотрудниками
-- Добавляем флаг IsDirectChat в таблицу CompanyChats

ALTER TABLE CompanyChats
    ADD COLUMN IsDirectChat TINYINT(1) NOT NULL DEFAULT 0;

-- Индекс для быстрого поиска личных чатов
CREATE INDEX IX_CompanyChats_IsDirectChat ON CompanyChats(IsDirectChat);
