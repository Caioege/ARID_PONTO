-- Script de alteração da tabela ConfiguracaoBonus
-- Data: 2026-03-29
-- Autor: Antigravity

ALTER TABLE ConfiguracaoBonus DROP COLUMN PagaEmFinaisDeSemanaEFeriados;
ALTER TABLE ConfiguracaoBonus ADD ApenasDiasComCargaHoraria BIT NOT NULL DEFAULT 0;
ALTER TABLE ConfiguracaoBonus ADD MinutosFaltaDesconto INT NULL;
ALTER TABLE ConfiguracaoBonus ADD MinutosFaltaDescontoMensal INT NULL;