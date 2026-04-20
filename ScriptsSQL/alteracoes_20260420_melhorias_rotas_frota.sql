-- Migration para Melhorias no Gerenciamento de Frota (Rotas)
-- Adição de Motorista Secundário

ALTER TABLE rota ADD COLUMN MotoristaSecundarioId INT NULL;
ALTER TABLE rota ADD CONSTRAINT fk_rota_motoristasecundario FOREIGN KEY (MotoristaSecundarioId) REFERENCES motorista(Id);
