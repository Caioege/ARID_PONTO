-- Execucao unificada de rotas para o aplicativo do motorista
-- Objetivo:
-- 1. Separar cadastro base da rota do estado operacional
-- 2. Persistir telemetria por execucao, nao por rota
-- 3. Auditar cada interacao relevante com data/hora e localizacao
-- 4. Remover colunas espalhadas e historicos em JSON como fonte primaria

-- Observacao:
-- Este script representa o desenho alvo para a nova implementacao.
-- A migracao de dados do legado deve ser tratada em etapa posterior, de forma controlada.

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `arid_ponto`.`rotaexecucaodesvio`;
DROP TABLE IF EXISTS `arid_ponto`.`rotaexecucaopausa`;
DROP TABLE IF EXISTS `arid_ponto`.`rotaexecucaoevento`;
DROP TABLE IF EXISTS `arid_ponto`.`rotaexecucaolocalizacao`;
DROP TABLE IF EXISTS `arid_ponto`.`paradaexecucao`;
DROP TABLE IF EXISTS `arid_ponto`.`localizacaorota`;
DROP TABLE IF EXISTS `arid_ponto`.`rotaexecucao`;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE `arid_ponto`.`rotaexecucao` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `MotoristaId` INT NOT NULL,
  `VeiculoId` INT NOT NULL,
  `ChecklistExecucaoId` INT NULL,
  `Status` INT NOT NULL COMMENT '0=Planejada,1=EmAndamento,2=Pausada,3=Finalizada,4=Cancelada',
  `DataHoraInicio` DATETIME NOT NULL,
  `DataHoraFim` DATETIME NULL,
  `UsuarioIdInicio` INT NULL,
  `UsuarioIdFim` INT NULL,
  `ObservacaoInicio` TEXT NULL,
  `ObservacaoFim` TEXT NULL,
  `UltimaLatitude` VARCHAR(50) NULL,
  `UltimaLongitude` VARCHAR(50) NULL,
  `UltimaAtualizacaoEm` DATETIME NULL,
  `GpsSimuladoUltimaLeitura` TINYINT(1) NOT NULL DEFAULT 0,
  `DataCriacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DataAlteracao` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_RotaExecucao_Organizacao_Status` (`OrganizacaoId`, `Status`),
  INDEX `IX_RotaExecucao_Rota` (`RotaId`),
  INDEX `IX_RotaExecucao_Motorista` (`MotoristaId`),
  INDEX `IX_RotaExecucao_Veiculo` (`VeiculoId`),
  INDEX `IX_RotaExecucao_Checklist` (`ChecklistExecucaoId`),
  CONSTRAINT `FK_RotaExecucao_Organizacao_v2`
    FOREIGN KEY (`OrganizacaoId`) REFERENCES `arid_ponto`.`organizacao` (`Id`),
  CONSTRAINT `FK_RotaExecucao_Rota_v2`
    FOREIGN KEY (`RotaId`) REFERENCES `arid_ponto`.`rota` (`Id`),
  CONSTRAINT `FK_RotaExecucao_Motorista_v2`
    FOREIGN KEY (`MotoristaId`) REFERENCES `arid_ponto`.`motorista` (`Id`),
  CONSTRAINT `FK_RotaExecucao_Veiculo_v2`
    FOREIGN KEY (`VeiculoId`) REFERENCES `arid_ponto`.`veiculo` (`Id`),
  CONSTRAINT `FK_RotaExecucao_Checklist_v2`
    FOREIGN KEY (`ChecklistExecucaoId`) REFERENCES `arid_ponto`.`checklistexecucao` (`Id`),
  CONSTRAINT `FK_RotaExecucao_UsuarioInicio_v2`
    FOREIGN KEY (`UsuarioIdInicio`) REFERENCES `arid_ponto`.`usuario` (`Id`),
  CONSTRAINT `FK_RotaExecucao_UsuarioFim_v2`
    FOREIGN KEY (`UsuarioIdFim`) REFERENCES `arid_ponto`.`usuario` (`Id`));

CREATE TABLE `arid_ponto`.`rotaexecucaolocalizacao` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaExecucaoId` INT NOT NULL,
  `Latitude` VARCHAR(50) NOT NULL,
  `Longitude` VARCHAR(50) NOT NULL,
  `PrecisaoEmMetros` DECIMAL(10,2) NULL,
  `VelocidadeMetrosPorSegundo` DECIMAL(10,2) NULL,
  `DirecaoGraus` DECIMAL(10,2) NULL,
  `AltitudeMetros` DECIMAL(10,2) NULL,
  `GpsSimulado` TINYINT(1) NOT NULL DEFAULT 0,
  `FonteCaptura` INT NOT NULL DEFAULT 0 COMMENT '0=Foreground,1=Background,2=Manual,3=Recuperacao',
  `DataHoraCaptura` DATETIME NOT NULL,
  `DataCriacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_RotaExecucaoLocalizacao_Execucao_DataHora` (`RotaExecucaoId`, `DataHoraCaptura`),
  CONSTRAINT `FK_RotaExecucaoLocalizacao_Organizacao`
    FOREIGN KEY (`OrganizacaoId`) REFERENCES `arid_ponto`.`organizacao` (`Id`),
  CONSTRAINT `FK_RotaExecucaoLocalizacao_Execucao`
    FOREIGN KEY (`RotaExecucaoId`) REFERENCES `arid_ponto`.`rotaexecucao` (`Id`) ON DELETE CASCADE);

CREATE TABLE `arid_ponto`.`rotaexecucaoevento` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaExecucaoId` INT NOT NULL,
  `ParadaRotaId` INT NULL,
  `UnidadeId` INT NULL,
  `Sequencia` INT NOT NULL DEFAULT 0,
  `TipoEvento` INT NOT NULL COMMENT '1=InicioRota,2=Origem,3=Parada,4=Destino,5=FimRota,6=ChecklistConfirmado,7=OcorrenciaManual',
  `StatusEvento` INT NULL COMMENT '0=Pendente,1=Confirmado,2=Recusado,3=Ignorado',
  `Entregue` TINYINT(1) NULL,
  `Observacao` TEXT NULL,
  `Latitude` VARCHAR(50) NULL,
  `Longitude` VARCHAR(50) NULL,
  `GpsSimulado` TINYINT(1) NOT NULL DEFAULT 0,
  `DataHoraEvento` DATETIME NOT NULL,
  `UsuarioIdRegistro` INT NULL,
  `DataCriacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_RotaExecucaoEvento_Execucao_Tipo_DataHora` (`RotaExecucaoId`, `TipoEvento`, `DataHoraEvento`),
  INDEX `IX_RotaExecucaoEvento_Parada` (`ParadaRotaId`),
  CONSTRAINT `FK_RotaExecucaoEvento_Organizacao`
    FOREIGN KEY (`OrganizacaoId`) REFERENCES `arid_ponto`.`organizacao` (`Id`),
  CONSTRAINT `FK_RotaExecucaoEvento_Execucao`
    FOREIGN KEY (`RotaExecucaoId`) REFERENCES `arid_ponto`.`rotaexecucao` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_RotaExecucaoEvento_ParadaRota`
    FOREIGN KEY (`ParadaRotaId`) REFERENCES `arid_ponto`.`paradarota` (`Id`),
  CONSTRAINT `FK_RotaExecucaoEvento_Unidade`
    FOREIGN KEY (`UnidadeId`) REFERENCES `arid_ponto`.`unidadeorganizacional` (`Id`),
  CONSTRAINT `FK_RotaExecucaoEvento_Usuario`
    FOREIGN KEY (`UsuarioIdRegistro`) REFERENCES `arid_ponto`.`usuario` (`Id`));

CREATE TABLE `arid_ponto`.`rotaexecucaopausa` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaExecucaoId` INT NOT NULL,
  `Motivo` VARCHAR(500) NOT NULL,
  `DataHoraInicio` DATETIME NOT NULL,
  `LatitudeInicio` VARCHAR(50) NULL,
  `LongitudeInicio` VARCHAR(50) NULL,
  `GpsSimuladoInicio` TINYINT(1) NOT NULL DEFAULT 0,
  `DataHoraFim` DATETIME NULL,
  `LatitudeFim` VARCHAR(50) NULL,
  `LongitudeFim` VARCHAR(50) NULL,
  `GpsSimuladoFim` TINYINT(1) NOT NULL DEFAULT 0,
  `UsuarioIdRegistro` INT NULL,
  `DataCriacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_RotaExecucaoPausa_Execucao` (`RotaExecucaoId`),
  CONSTRAINT `FK_RotaExecucaoPausa_Organizacao`
    FOREIGN KEY (`OrganizacaoId`) REFERENCES `arid_ponto`.`organizacao` (`Id`),
  CONSTRAINT `FK_RotaExecucaoPausa_Execucao`
    FOREIGN KEY (`RotaExecucaoId`) REFERENCES `arid_ponto`.`rotaexecucao` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_RotaExecucaoPausa_Usuario`
    FOREIGN KEY (`UsuarioIdRegistro`) REFERENCES `arid_ponto`.`usuario` (`Id`));

CREATE TABLE `arid_ponto`.`rotaexecucaodesvio` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaExecucaoId` INT NOT NULL,
  `RotaExecucaoLocalizacaoId` BIGINT NULL,
  `Latitude` VARCHAR(50) NOT NULL,
  `Longitude` VARCHAR(50) NOT NULL,
  `DistanciaEmMetros` DECIMAL(10,2) NOT NULL,
  `DataHoraDeteccao` DATETIME NOT NULL,
  `Observacao` VARCHAR(500) NULL,
  `DataCriacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_RotaExecucaoDesvio_Execucao` (`RotaExecucaoId`),
  CONSTRAINT `FK_RotaExecucaoDesvio_Organizacao`
    FOREIGN KEY (`OrganizacaoId`) REFERENCES `arid_ponto`.`organizacao` (`Id`),
  CONSTRAINT `FK_RotaExecucaoDesvio_Execucao`
    FOREIGN KEY (`RotaExecucaoId`) REFERENCES `arid_ponto`.`rotaexecucao` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_RotaExecucaoDesvio_Localizacao`
    FOREIGN KEY (`RotaExecucaoLocalizacaoId`) REFERENCES `arid_ponto`.`rotaexecucaolocalizacao` (`Id`) ON DELETE SET NULL);

-- Regras de consistencia operacional
-- 1. Em uma mesma execucao, origem, destino e cada parada sao confirmados via rotaexecucaoevento.
-- 2. Telemetria continua vai para rotaexecucaolocalizacao.
-- 3. Pausas deixam de ser JSON e passam a ser linhas auditaveis.
-- 4. Desvios deixam de ser apenas por rota e passam a apontar para a execucao real.
