-- Миграция: таблица ключей Data Protection для сохранения авторизации между перезапусками сервера

CREATE TABLE IF NOT EXISTS DataProtectionKeys (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FriendlyName TEXT NULL,
    Xml LONGTEXT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
