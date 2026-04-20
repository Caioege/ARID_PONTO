-- Adicionar suporte para Pausas em Rotas

-- Tabela Rota
ALTER TABLE rota 
ADD COLUMN PermitePausa BIT(1) NOT NULL DEFAULT b'0',
ADD COLUMN QuantidadePausas INT NOT NULL DEFAULT 0;

-- Tabela RotaExecucao
ALTER TABLE rotaexecucao 
ADD COLUMN HistoricoPausas TEXT NULL;
