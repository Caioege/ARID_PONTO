CREATE TABLE ConfiguracaoBonus (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrganizacaoId INT NOT NULL,
    Descricao VARCHAR(100) NOT NULL,
    ValorDiario DECIMAL(18, 2) NOT NULL,
    PagaEmFinaisDeSemanaEFeriados TINYINT(1) NOT NULL DEFAULT 0,
    TurnoIntercaladoPagaDobrado TINYINT(1) NOT NULL DEFAULT 0,
    Ativo TINYINT(1) NOT NULL DEFAULT 1,
    KEY IX_ConfiguracaoBonus_OrganizacaoId (OrganizacaoId)
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
        FOREIGN KEY (VinculoDeTrabalhoId) REFERENCES VinculosDeTrabalho (Id) ON DELETE CASCADE,
    CONSTRAINT FK_BonusCalculado_ConfiguracaoBonus 
        FOREIGN KEY (ConfiguracaoBonusId) REFERENCES ConfiguracaoBonus (Id) ON DELETE CASCADE
);
