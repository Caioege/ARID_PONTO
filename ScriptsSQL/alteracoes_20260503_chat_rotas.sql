-- Chat textual entre o sistema web e o aplicativo durante a execução da rota.
CREATE TABLE IF NOT EXISTS rotaexecucaochat (
    Id INT NOT NULL AUTO_INCREMENT,
    OrganizacaoId INT NOT NULL,
    RotaExecucaoId INT NOT NULL,
    RotaId INT NOT NULL,
    Origem TINYINT NOT NULL COMMENT '1 = Sistema web, 2 = Aplicativo',
    UsuarioId INT NULL,
    ServidorId INT NULL,
    RemetenteNome VARCHAR(255) NOT NULL,
    Mensagem VARCHAR(1000) NOT NULL,
    DataHoraEnvio DATETIME NOT NULL,
    LidaNoSistema BOOLEAN NOT NULL DEFAULT 0,
    DataHoraLeituraSistema DATETIME NULL,
    LidaNoAplicativo BOOLEAN NOT NULL DEFAULT 0,
    DataHoraLeituraAplicativo DATETIME NULL,
    PRIMARY KEY (Id),
    INDEX IX_rotaexecucaochat_execucao (RotaExecucaoId, DataHoraEnvio),
    INDEX IX_rotaexecucaochat_rota (RotaId),
    INDEX IX_rotaexecucaochat_organizacao (OrganizacaoId),
    CONSTRAINT FK_rotaexecucaochat_execucao FOREIGN KEY (RotaExecucaoId) REFERENCES rotaexecucao(Id),
    CONSTRAINT FK_rotaexecucaochat_rota FOREIGN KEY (RotaId) REFERENCES rota(Id),
    CONSTRAINT FK_rotaexecucaochat_organizacao FOREIGN KEY (OrganizacaoId) REFERENCES organizacao(Id)
);
