-- Migration para Suporte a Recorrência Periódica e Dias da Semana (Flags)

ALTER TABLE rota ADD COLUMN DataInicio DATETIME NULL;
ALTER TABLE rota ADD COLUMN DataFim DATETIME NULL;
ALTER TABLE rota ADD COLUMN DiasSemana INT NULL DEFAULT 0;
