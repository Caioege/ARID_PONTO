-- =========================================================================
-- Script de Criação: Configuração de Liveness e Atestado no Aplicativo
-- Banco de Dados: MySQL
-- =========================================================================

-- 1. Adicionado Enum/Configuração no Servidor com Valor Padrão 2 (LivenessFacial)
ALTER TABLE `Servidor` 
ADD COLUMN `TipoComprovacaoPontoApp` INT NOT NULL DEFAULT 2;

-- 2. Tabela de Histórico de Mudança na Configuração de App
CREATE TABLE `HistoricoConfiguracaoAppServidor` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `ServidorId` INT NOT NULL,
    `TipoComprovacaoAnterior` INT NOT NULL,
    `TipoComprovacaoNova` INT NOT NULL,
    `Motivo` VARCHAR(500) NOT NULL,
    `DataAlteracao` DATETIME NOT NULL,
    `UsuarioAlteracaoId` INT NOT NULL,
    
    -- Colunas do tipo EntidadeOrganizacaoBase
    `OrganizacaoId` INT NOT NULL,
    `InativoDesde` DATETIME NULL,
    `Inativo` TINYINT(1) NOT NULL DEFAULT 0,
    
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_HistoricoConfiguracaoAppServidor_Servidor` FOREIGN KEY (`ServidorId`) REFERENCES `Servidor` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_HistoricoConfiguracaoAppServidor_Usuario` FOREIGN KEY (`UsuarioAlteracaoId`) REFERENCES `Usuario` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_HistoricoConfiguracaoAppServidor_Organizacao` FOREIGN KEY (`OrganizacaoId`) REFERENCES `Organizacao` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 3. Inclusão da Coluna para Anexo do Atestado (PDF/Imagem)
ALTER TABLE `RegistroAplicativo`
ADD COLUMN `ComprovanteAtestado` VARCHAR(255) NULL;
