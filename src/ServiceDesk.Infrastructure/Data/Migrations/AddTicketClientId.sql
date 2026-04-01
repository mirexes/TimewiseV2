-- Миграция: добавление поля ClientId в таблицу Tickets
ALTER TABLE Tickets
    ADD COLUMN ClientId INT NULL COMMENT 'Клиент (организация), к которой относится заявка'
    AFTER CreatedByUserId;

ALTER TABLE Tickets
    ADD CONSTRAINT FK_Tickets_Clients_ClientId
    FOREIGN KEY (ClientId) REFERENCES Clients(Id)
    ON DELETE SET NULL;

CREATE INDEX idx_tickets_client ON Tickets(ClientId);

-- Заполнение ClientId для существующих заявок на основе связи точки обслуживания с клиентом
UPDATE Tickets t
    JOIN ClientServicePoints csp ON csp.ServicePointId = t.ServicePointId
SET t.ClientId = csp.ClientId
WHERE t.ClientId IS NULL;
