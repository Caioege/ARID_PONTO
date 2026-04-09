-- Migration para Suporte a Geofencing e Otimização TSP (OSRM) 

-- Adição do campo para armazenar o trajeto validado geométrico
ALTER TABLE rota ADD COLUMN PolylineOficial TEXT NULL;
ALTER TABLE rota ADD COLUMN Observacao TEXT NULL;

-- Adição do campo de ordenação sequencial as paradas para TSP
ALTER TABLE paradarota ADD COLUMN Ordem INT NOT NULL DEFAULT 0;

-- Criação da tabela de auditoria de desvios geográficos via Haversine
CREATE TABLE IF NOT EXISTS rotaocorrenciadesvio (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RotaId INT NOT NULL,
    Latitude VARCHAR(50) NOT NULL,
    Longitude VARCHAR(50) NOT NULL,
    DataHora DATETIME NOT NULL,
    DistanciaEmMetros DOUBLE NOT NULL,
    OrganizacaoId INT NOT NULL,
    Ativo TINYINT(1) NOT NULL DEFAULT 1,
    UsuarioCriacaoId INT NULL,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAlteracaoId INT NULL,
    DataAlteracao DATETIME NULL,
    FOREIGN KEY (RotaId) REFERENCES rota(Id),
    FOREIGN KEY (OrganizacaoId) REFERENCES organizacao(Id),
    FOREIGN KEY (UsuarioCriacaoId) REFERENCES usuario(Id),
    FOREIGN KEY (UsuarioAlteracaoId) REFERENCES usuario(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
