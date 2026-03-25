-- Добавление метки безопасности для инвалидации сессий при деактивации
ALTER TABLE Users ADD COLUMN IF NOT EXISTS SecurityStamp VARCHAR(50) NOT NULL DEFAULT (UUID());
