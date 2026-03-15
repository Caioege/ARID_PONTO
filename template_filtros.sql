-- ARI-D GERENCIAMENTO DE PONTO
-- SCRIPT DE CRIAÇÃO: TEMPLATES DE FILTROS (ITEM 1 TR)
-- DATA: 2026-03-15

-- Tabela para armazenar as preferências de filtros dos relatórios
CREATE TABLE IF NOT EXISTS FiltroRelatorio (
    Id INT NOT NULL AUTO_INCREMENT,
    OrganizacaoId INT NOT NULL,
    UsuarioCriadorId INT NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    UrlRelatorio VARCHAR(255) NOT NULL, -- Ex: /Relatorio/ListaDeServidores
    JsonParametros TEXT NOT NULL,       -- JSON contendo chaves e valores dos inputs (ex: {"UnidadeId": "1", "HorarioId": "2"})
    Compartilhado TINYINT(1) NOT NULL DEFAULT 0, -- 1 = Visível para outros operadores, 0 = Privado
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    PRIMARY KEY (Id),
    INDEX IX_FiltroRelatorio_Organizacao (OrganizacaoId),
    INDEX IX_FiltroRelatorio_Usuario (UsuarioCriadorId),
    
    CONSTRAINT FK_FiltroRelatorio_Organizacao FOREIGN KEY (OrganizacaoId) REFERENCES Organizacao(Id),
    CONSTRAINT FK_FiltroRelatorio_Usuario FOREIGN KEY (UsuarioCriadorId) REFERENCES Usuario(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Comentários técnicos:
-- A coluna JsonParametros armazena o estado do formulário de filtros no momento do salvamento.
-- Isso permite adicionar novos campos de filtro no futuro sem precisar alterar o esquema do banco.
