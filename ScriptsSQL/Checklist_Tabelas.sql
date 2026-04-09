-- Tabela de Itens de Checklist (Cadastro)
CREATE TABLE IF NOT EXISTS ChecklistItem (
    Id INT NOT NULL AUTO_INCREMENT,
    OrganizacaoId INT NOT NULL,
    VeiculoId INT NOT NULL,
    Descricao VARCHAR(255) NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    PRIMARY KEY (Id),
    CONSTRAINT FK_ChecklistItem_Organizacao FOREIGN KEY (OrganizacaoId) REFERENCES Organizacao(Id),
    CONSTRAINT FK_ChecklistItem_Veiculo FOREIGN KEY (VeiculoId) REFERENCES Veiculo(Id)
);

-- Tabela de Execução de Checklist (Log do Aplicativo)
CREATE TABLE IF NOT EXISTS ChecklistExecucao (
    Id INT NOT NULL AUTO_INCREMENT,
    OrganizacaoId INT NOT NULL,
    VeiculoId INT NOT NULL,
    MotoristaId INT NOT NULL,
    RotaId INT NULL,
    DataHora DATETIME NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_ChecklistExecucao_Organizacao FOREIGN KEY (OrganizacaoId) REFERENCES Organizacao(Id),
    CONSTRAINT FK_ChecklistExecucao_Veiculo FOREIGN KEY (VeiculoId) REFERENCES Veiculo(Id),
    CONSTRAINT FK_ChecklistExecucao_Motorista FOREIGN KEY (MotoristaId) REFERENCES Motorista(Id),
    CONSTRAINT FK_ChecklistExecucao_Rota FOREIGN KEY (RotaId) REFERENCES Rota(Id)
);

-- Tabela de Itens Marcados na Execução
CREATE TABLE IF NOT EXISTS ChecklistExecucaoItem (
    Id INT NOT NULL AUTO_INCREMENT,
    ChecklistExecucaoId INT NOT NULL,
    ChecklistItemId INT NOT NULL,
    Marcado BIT NOT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_ChecklistExecucaoItem_Execucao FOREIGN KEY (ChecklistExecucaoId) REFERENCES ChecklistExecucao(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ChecklistExecucaoItem_Item FOREIGN KEY (ChecklistItemId) REFERENCES ChecklistItem(Id)
);

-- Permissões Padrão para Checklist (Exemplo para Administrador de Sistema ou Gerente)
-- NOTE: O sistema já possui um controle de permissões dinâmico em GrupoDePermissao e ItemDoGrupoDePermissao.
-- Os enums criados no código (eItemDePermissao_Checklist) serão usados para validar no controller.
