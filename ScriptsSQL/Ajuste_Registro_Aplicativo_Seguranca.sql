-- =========================================================================
-- Script de Atualização: Colunas de Segurança e Auditoria (Liveness/GPS)
-- Tabela: RegistroAplicativo
-- Banco de Dados: MySQL
-- =========================================================================

ALTER TABLE `RegistroAplicativo`
ADD COLUMN `MockGPS` TINYINT(1) NOT NULL DEFAULT 0,
ADD COLUMN `LivenessSuccess` TINYINT(1) NOT NULL DEFAULT 0,
ADD COLUMN `AnexoLiveness` VARCHAR(255) NULL,
ADD COLUMN `MotivoAuditoria` VARCHAR(1000) NULL;

-- Garante que a coluna de atestado exista (caso não tenha sido criada)
-- ALTER TABLE `RegistroAplicativo` ADD COLUMN `ComprovanteAtestado` VARCHAR(255) NULL;
