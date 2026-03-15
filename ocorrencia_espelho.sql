CREATE TABLE OcorrenciaDoEspelhoPonto (
    Id INT  PRIMARY KEY auto_increment,
    OrganizacaoId INT NOT NULL,
    VinculoDeTrabalhoId INT NOT NULL,
    MesReferencia VARCHAR(7) NOT NULL, -- Formato MM/yyyy
    Descricao VARCHAR(8000) NOT NULL,
    DataHoraCadastro DATETIME NOT NULL,
    UsuarioCadastroId INT NOT NULL,
    UsuarioCadastroNome VARCHAR(150) NOT NULL,
    
    CONSTRAINT FK_OcorrenciaEspelho_Organizacao FOREIGN KEY (OrganizacaoId) REFERENCES Organizacao(Id),
    CONSTRAINT FK_OcorrenciaEspelho_Vinculo FOREIGN KEY (VinculoDeTrabalhoId) REFERENCES VinculoDeTrabalho(Id),
    CONSTRAINT FK_OcorrenciaEspelho_Usuario FOREIGN KEY (UsuarioCadastroId) REFERENCES Usuario(Id)
);
