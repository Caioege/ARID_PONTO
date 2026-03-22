-- Alterações de Banco de Dados: Sistema de Ponto
-- Gerado em: 2026-03-15
-- Descrição: Adição de novas colunas para o controle de tolerância por marcação e base para Relatório de Absenteísmo.

-- 1. Cadastro de Tolerâncias por Jornada
-- Tabela: horariodetrabalhovigencia
-- Motivo: As tolerâncias antes eram apenas diárias ou de DSR. Agora precisam ser aplicadas antes/após a entrada e antes/após a saída de cada período.
ALTER TABLE `horariodetrabalhovigencia` 
ADD COLUMN `ToleranciaAntesDaEntradaEmMinutos` INT NOT NULL DEFAULT 0 AFTER `ToleranciaDsrEmMinutos`,
ADD COLUMN `ToleranciaAposAEntradaEmMinutos` INT NOT NULL DEFAULT 0 AFTER `ToleranciaAntesDaEntradaEmMinutos`,
ADD COLUMN `ToleranciaAntesDaSaidaEmMinutos` INT NOT NULL DEFAULT 0 AFTER `ToleranciaAposAEntradaEmMinutos`,
ADD COLUMN `ToleranciaAposASaidaEmMinutos` INT NOT NULL DEFAULT 0 AFTER `ToleranciaAntesDaSaidaEmMinutos`;

-- 2. Busca de Horários Similares
-- Nenhuma alteração estrutural necessária. A busca baseia-se nas tabelas existentes (HorarioDeTrabalho, HorarioDeTrabalhoVigencia, HorarioDeTrabalhoDia).

-- 3. Relatório de Absenteísmo
-- Nenhuma alteração estrutural necessária. A consulta lerá os dados de PontoDoDia (HorasNegativas > 0) e VinculoDeTrabalho.
