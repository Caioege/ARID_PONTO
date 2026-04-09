-- 1. Tabela de Pacientes
CREATE TABLE IF NOT EXISTS Paciente (
    Id INT NOT NULL AUTO_INCREMENT,
    OrganizacaoId INT NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    CPF VARCHAR(14) NULL,
    DataNascimento DATETIME NULL,
    Telefone VARCHAR(15) NULL,
    AcompanhanteNome VARCHAR(150) NULL,
    AcompanhanteCPF VARCHAR(14) NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    Inativo BIT NOT NULL DEFAULT 0,
    DataInclusao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    CONSTRAINT FK_Paciente_Organizacao FOREIGN KEY (OrganizacaoId) REFERENCES Organizacao(Id)
);

-- 2. Tabela de Manutenção de Veículo
CREATE TABLE IF NOT EXISTS ManutencaoVeiculo (
    Id INT NOT NULL AUTO_INCREMENT,
    OrganizacaoId INT NOT NULL,
    VeiculoId INT NOT NULL,
    DataManutencao DATETIME NOT NULL,
    Descricao VARCHAR(100) NOT NULL,
    KmNaManutencao INT NOT NULL,
    KmProximaManutencao INT NULL,	
    DataVencimentoManutencao DATETIME NULL,
    Observacao TEXT NULL,
    Inativo BIT NOT NULL DEFAULT 0,
    DataInclusao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id),
    CONSTRAINT FK_ManutencaoVeiculo_Organizacao FOREIGN KEY (OrganizacaoId) REFERENCES Organizacao(Id),
    CONSTRAINT FK_ManutencaoVeiculo_Veiculo FOREIGN KEY (VeiculoId) REFERENCES Veiculo(Id)
);

-- 3. Junction Tables para Rota
CREATE TABLE IF NOT EXISTS RotaPaciente (
    Id INT NOT NULL AUTO_INCREMENT,
    RotaId INT NOT NULL,
    PacienteId INT NOT NULL,
    PossuiAcompanhante BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    CONSTRAINT FK_RotaPaciente_Rota FOREIGN KEY (RotaId) REFERENCES Rota(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RotaPaciente_Paciente FOREIGN KEY (PacienteId) REFERENCES Paciente(Id)
);

CREATE TABLE IF NOT EXISTS RotaProfissional (
    Id INT NOT NULL AUTO_INCREMENT,
    RotaId INT NOT NULL,
    ServidorId INT NOT NULL,
    Observacao VARCHAR(255) NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_RotaProfissional_Rota FOREIGN KEY (RotaId) REFERENCES Rota(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RotaProfissional_Servidor FOREIGN KEY (ServidorId) REFERENCES Servidor(Id)
);

-- 4. Alterações na Tabela Rota
-- Nota: ALguns campos podem já existir de scripts anteriores parciais, usamos IGNORE ou checamos.
ALTER TABLE Rota ADD COLUMN UnidadeDestinoId INT NULL;
ALTER TABLE Rota ADD CONSTRAINT FK_Rota_UnidadeDestino FOREIGN KEY (UnidadeDestinoId) REFERENCES UnidadeOrganizacional(Id);
