-- Alteracoes de Banco de Dados: Sistema de Ponto
-- Gerado em: 2026-03-22
-- Descricao: Criacao das tabelas base do modulo Gestao Mobile (Rotas e Paradas) e flag na Organizacao.

-- 1. Habilitando modulo na Organizacao
ALTER TABLE `arid_ponto`.`organizacao`
ADD COLUMN `GestaoMobileAtivo` TINYINT NOT NULL DEFAULT 0;

-- 2. Tabela Veiculo
CREATE TABLE `arid_ponto`.`veiculo` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `Placa` VARCHAR(20) NOT NULL,
  `Marca` VARCHAR(100) NOT NULL,
  `Modelo` VARCHAR(100) NOT NULL,
  `Cor` INT NOT NULL,
  `TipoCombustivel` INT NOT NULL,
  `Status` INT NOT NULL,
  `AnoFabricacao` INT NOT NULL,
  `AnoModelo` INT NOT NULL,
  `Renavam` VARCHAR(50) NULL,
  `Chassi` VARCHAR(50) NULL,
  `QuilometragemAtual` INT NOT NULL DEFAULT 0,
  `VencimentoLicenciamento` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Veiculo_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  CONSTRAINT `FK_Veiculo_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
);

-- 3. Tabela Motorista
CREATE TABLE `arid_ponto`.`motorista` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `ServidorId` INT NOT NULL,
  `NumeroCNH` VARCHAR(50) NOT NULL,
  `CategoriaCNH` INT NOT NULL,
  `EmissaoCNH` DATETIME NOT NULL,
  `VencimentoCNH` DATETIME NOT NULL,
  `Situacao` INT NOT NULL,
  `Observacoes` VARCHAR(500) NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Motorista_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_Motorista_Servidor_idx` (`ServidorId` ASC) VISIBLE,
  CONSTRAINT `FK_Motorista_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Motorista_Servidor`
    FOREIGN KEY (`ServidorId`)
    REFERENCES `arid_ponto`.`servidor` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
);

-- 4. Tabela Rota
CREATE TABLE `arid_ponto`.`rota` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `MotoristaId` INT NOT NULL,
  `VeiculoId` INT NULL,
  `Descricao` VARCHAR(100) NOT NULL,
  `Situacao` INT NOT NULL DEFAULT 0,
  `Recorrente` TINYINT NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `FK_Rota_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_Rota_Motorista_idx` (`MotoristaId` ASC) VISIBLE,
  INDEX `FK_Rota_Veiculo_idx` (`VeiculoId` ASC) VISIBLE,
  CONSTRAINT `FK_Rota_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Rota_Motorista`
    FOREIGN KEY (`MotoristaId`)
    REFERENCES `arid_ponto`.`motorista` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Rota_Veiculo`
    FOREIGN KEY (`VeiculoId`)
    REFERENCES `arid_ponto`.`veiculo` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
);

-- 5. Tabela ParadaRota
CREATE TABLE `arid_ponto`.`paradarota` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `Endereco` VARCHAR(250) NOT NULL,
  `Latitude` VARCHAR(50) NULL,
  `Longitude` VARCHAR(50) NULL,
  `Link` VARCHAR(500) NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_ParadaRota_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_ParadaRota_Rota_idx` (`RotaId` ASC) VISIBLE,
  CONSTRAINT `FK_ParadaRota_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_ParadaRota_Rota`
    FOREIGN KEY (`RotaId`)
    REFERENCES `arid_ponto`.`rota` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

-- Nota: Os enumeradores de permissao sao definidos em codigo e salvos pela interface.

-- 6. Tabela MotoristaHistoricoSituacao
CREATE TABLE `arid_ponto`.`motoristahistoricosituacao` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `MotoristaId` INT NOT NULL,
  `UsuarioId` INT NOT NULL,
  `SituacaoAnterior` INT NOT NULL,
  `SituacaoNova` INT NOT NULL,
  `DataAlteracao` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_MHistSituacao_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_MHistSituacao_Motorista_idx` (`MotoristaId` ASC) VISIBLE,
  INDEX `FK_MHistSituacao_Usuario_idx` (`UsuarioId` ASC) VISIBLE,
  CONSTRAINT `FK_MHistSituacao_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_MHistSituacao_Motorista`
    FOREIGN KEY (`MotoristaId`)
    REFERENCES `arid_ponto`.`motorista` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `FK_MHistSituacao_Usuario`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `arid_ponto`.`usuario` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
);

-- A estrutura operacional de execucao de rotas foi movida para:
-- ScriptsSQL/alteracoes_20260428_execucao_rotas_motorista_unificada.sql
