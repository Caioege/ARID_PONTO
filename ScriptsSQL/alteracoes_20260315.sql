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

-- 4. Tabelas do Módulo de Bônus (VT/VA)
CREATE TABLE ConfiguracaoBonus (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrganizacaoId INT NOT NULL,
    Descricao VARCHAR(100) NOT NULL,
    ValorDiario DECIMAL(18, 2) NOT NULL,
    TipoBonus INT NOT NULL DEFAULT 1 COMMENT '1 = Diario, 2 = Mensal Fixo',
    PerdeIntegralmenteComFalta TINYINT(1) NOT NULL DEFAULT 1,
    PagaEmFinaisDeSemanaEFeriados TINYINT(1) NOT NULL DEFAULT 0,
    TurnoIntercaladoPagaDobrado TINYINT(1) NOT NULL DEFAULT 0,
    MinutosIntervaloTurnoIntercalado INT NOT NULL DEFAULT 120,
    Ativo TINYINT(1) NOT NULL DEFAULT 1,
    KEY IX_ConfiguracaoBonus_OrganizacaoId (OrganizacaoId)
);

CREATE TABLE ConfiguracaoBonusFuncao (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ConfiguracaoBonusId INT NOT NULL,
    FuncaoId INT NOT NULL,
    KEY IX_ConfiguracaoBonusFuncao_ConfiguracaoBonusId (ConfiguracaoBonusId),
    KEY IX_ConfiguracaoBonusFuncao_FuncaoId (FuncaoId),
    CONSTRAINT FK_ConfiguracaoBonusFuncao_ConfiguracaoBonus 
        FOREIGN KEY (ConfiguracaoBonusId) REFERENCES ConfiguracaoBonus (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ConfiguracaoBonusFuncao_Funcao 
        FOREIGN KEY (FuncaoId) REFERENCES Funcao (Id) ON DELETE CASCADE
);

CREATE TABLE BonusCalculado (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrganizacaoId INT NOT NULL,
    VinculoDeTrabalhoId INT NOT NULL,
    ConfiguracaoBonusId INT NOT NULL,
    MesReferencia VARCHAR(10) NOT NULL,
    DiasEfetivosTrabalhados INT NOT NULL,
    DiasTurnoIntercalado INT NOT NULL,
    ValorTotal DECIMAL(18, 2) NOT NULL,
    DetalhesDoCalculoJson TEXT NULL,
    DataCalculo DATETIME NOT NULL,
    KEY IX_BonusCalculado_OrganizacaoId (OrganizacaoId),
    KEY IX_BonusCalculado_VinculoDeTrabalhoId (VinculoDeTrabalhoId),
    KEY IX_BonusCalculado_ConfiguracaoBonusId (ConfiguracaoBonusId),
    CONSTRAINT FK_BonusCalculado_VinculosDeTrabalho 
        FOREIGN KEY (VinculoDeTrabalhoId) REFERENCES VinculoDeTrabalho (Id) ON DELETE CASCADE,
    CONSTRAINT FK_BonusCalculado_ConfiguracaoBonus 
        FOREIGN KEY (ConfiguracaoBonusId) REFERENCES ConfiguracaoBonus (Id) ON DELETE CASCADE
);
