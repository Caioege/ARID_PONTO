-- Auditoria e contratos de sincronizacao offline para o aplicativo de rastreio
-- Data: 03/05/2026
--
-- Objetivo:
-- 1. Identificar execucoes com registros offline.
-- 2. Marcar localizacoes, eventos, pausas e desvios gerados offline.
-- 3. Registrar controle de idempotencia por LocalExecucaoId e ClientEventId.
-- 4. Sustentar o alerta de monitoramento "Possivelmente offline" por DataHoraUltimaComunicacaoApp.

ALTER TABLE `arid_ponto`.`rotaexecucao`
  ADD COLUMN `PossuiRegistroOffline` TINYINT(1) NOT NULL DEFAULT 0 AFTER `GpsSimuladoUltimaLeitura`,
  ADD COLUMN `ExecucaoOfflineCompleta` TINYINT(1) NOT NULL DEFAULT 0 AFTER `PossuiRegistroOffline`,
  ADD COLUMN `DataHoraPrimeiroRegistroOffline` DATETIME NULL AFTER `ExecucaoOfflineCompleta`,
  ADD COLUMN `DataHoraUltimoRegistroOffline` DATETIME NULL AFTER `DataHoraPrimeiroRegistroOffline`,
  ADD COLUMN `DataHoraUltimaComunicacaoApp` DATETIME NULL AFTER `DataHoraUltimoRegistroOffline`,
  ADD COLUMN `LocalExecucaoId` VARCHAR(100) NULL AFTER `DataHoraUltimaComunicacaoApp`,
  ADD COLUMN `IdentificadorDispositivo` VARCHAR(150) NULL AFTER `LocalExecucaoId`,
  ADD INDEX `IX_RotaExecucao_ComunicacaoApp` (`Status`, `DataHoraUltimaComunicacaoApp`),
  ADD INDEX `IX_RotaExecucao_Offline` (`PossuiRegistroOffline`, `ExecucaoOfflineCompleta`),
  ADD UNIQUE INDEX `UX_RotaExecucao_LocalExecucaoId` (`LocalExecucaoId`);

ALTER TABLE `arid_ponto`.`rotaexecucaolocalizacao`
  ADD COLUMN `RegistradoOffline` TINYINT(1) NOT NULL DEFAULT 0 AFTER `DataHoraCaptura`,
  ADD COLUMN `DataHoraRegistroLocal` DATETIME NULL AFTER `RegistradoOffline`,
  ADD COLUMN `DataHoraSincronizacao` DATETIME NULL AFTER `DataHoraRegistroLocal`,
  ADD COLUMN `IdentificadorDispositivo` VARCHAR(150) NULL AFTER `DataHoraSincronizacao`,
  ADD COLUMN `LocalExecucaoId` VARCHAR(100) NULL AFTER `IdentificadorDispositivo`,
  ADD COLUMN `ClientEventId` VARCHAR(100) NULL AFTER `LocalExecucaoId`,
  ADD INDEX `IX_RotaExecucaoLocalizacao_Offline` (`RotaExecucaoId`, `RegistradoOffline`, `DataHoraCaptura`),
  ADD UNIQUE INDEX `UX_RotaExecucaoLocalizacao_ClientEvent` (`ClientEventId`);

ALTER TABLE `arid_ponto`.`rotaexecucaoevento`
  ADD COLUMN `RegistradoOffline` TINYINT(1) NOT NULL DEFAULT 0 AFTER `DataHoraEvento`,
  ADD COLUMN `DataHoraRegistroLocal` DATETIME NULL AFTER `RegistradoOffline`,
  ADD COLUMN `DataHoraSincronizacao` DATETIME NULL AFTER `DataHoraRegistroLocal`,
  ADD COLUMN `IdentificadorDispositivo` VARCHAR(150) NULL AFTER `DataHoraSincronizacao`,
  ADD COLUMN `LocalExecucaoId` VARCHAR(100) NULL AFTER `IdentificadorDispositivo`,
  ADD COLUMN `ClientEventId` VARCHAR(100) NULL AFTER `LocalExecucaoId`,
  ADD INDEX `IX_RotaExecucaoEvento_Offline` (`RotaExecucaoId`, `RegistradoOffline`, `DataHoraEvento`),
  ADD UNIQUE INDEX `UX_RotaExecucaoEvento_ClientEvent` (`ClientEventId`);

ALTER TABLE `arid_ponto`.`rotaexecucaopausa`
  ADD COLUMN `RegistradoOffline` TINYINT(1) NOT NULL DEFAULT 0 AFTER `GpsSimuladoFim`,
  ADD COLUMN `DataHoraRegistroLocal` DATETIME NULL AFTER `RegistradoOffline`,
  ADD COLUMN `DataHoraSincronizacao` DATETIME NULL AFTER `DataHoraRegistroLocal`,
  ADD COLUMN `IdentificadorDispositivo` VARCHAR(150) NULL AFTER `DataHoraSincronizacao`,
  ADD COLUMN `LocalExecucaoId` VARCHAR(100) NULL AFTER `IdentificadorDispositivo`,
  ADD COLUMN `ClientEventId` VARCHAR(100) NULL AFTER `LocalExecucaoId`,
  ADD INDEX `IX_RotaExecucaoPausa_Offline` (`RotaExecucaoId`, `RegistradoOffline`, `DataHoraInicio`),
  ADD UNIQUE INDEX `UX_RotaExecucaoPausa_ClientEvent` (`ClientEventId`);

ALTER TABLE `arid_ponto`.`rotaexecucaodesvio`
  ADD COLUMN `RegistradoOffline` TINYINT(1) NOT NULL DEFAULT 0 AFTER `Observacao`,
  ADD COLUMN `DataHoraRegistroLocal` DATETIME NULL AFTER `RegistradoOffline`,
  ADD COLUMN `DataHoraSincronizacao` DATETIME NULL AFTER `DataHoraRegistroLocal`,
  ADD COLUMN `IdentificadorDispositivo` VARCHAR(150) NULL AFTER `DataHoraSincronizacao`,
  ADD COLUMN `LocalExecucaoId` VARCHAR(100) NULL AFTER `IdentificadorDispositivo`,
  ADD COLUMN `ClientEventId` VARCHAR(100) NULL AFTER `LocalExecucaoId`,
  ADD INDEX `IX_RotaExecucaoDesvio_Offline` (`RotaExecucaoId`, `RegistradoOffline`, `DataHoraDeteccao`),
  ADD UNIQUE INDEX `UX_RotaExecucaoDesvio_ClientEvent` (`ClientEventId`);

CREATE TABLE IF NOT EXISTS `arid_ponto`.`rotaexecucaosincronizacaooffline` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaExecucaoId` INT NOT NULL,
  `LocalExecucaoId` VARCHAR(100) NOT NULL,
  `ClientEventId` VARCHAR(100) NULL,
  `TipoRegistro` VARCHAR(50) NOT NULL,
  `IdentificadorDispositivo` VARCHAR(150) NULL,
  `DataHoraRegistroLocal` DATETIME NULL,
  `DataHoraSincronizacao` DATETIME NOT NULL,
  `DataCriacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `UX_RotaExecucaoSyncOffline_Evento` (`LocalExecucaoId`, `ClientEventId`, `TipoRegistro`),
  INDEX `IX_RotaExecucaoSyncOffline_Execucao` (`RotaExecucaoId`),
  INDEX `IX_RotaExecucaoSyncOffline_Organizacao` (`OrganizacaoId`),
  CONSTRAINT `FK_RotaExecucaoSyncOffline_Organizacao`
    FOREIGN KEY (`OrganizacaoId`) REFERENCES `arid_ponto`.`organizacao` (`Id`),
  CONSTRAINT `FK_RotaExecucaoSyncOffline_Execucao`
    FOREIGN KEY (`RotaExecucaoId`) REFERENCES `arid_ponto`.`rotaexecucao` (`Id`) ON DELETE CASCADE
);

ALTER TABLE `arid_ponto`.`paradarota`
  ADD COLUMN `ObservacaoCadastro` TEXT NULL AFTER `Link`;
