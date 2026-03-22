-- Alterações de Banco de Dados: Sistema de Ponto
-- Gerado em: 2026-03-22
-- Descrição: Criação das tabelas do módulo Gestão Mobile (Rotas, Paradas, Localização) e flag na Organização.

-- 1. Habilitando módulo na Organização
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
    ON UPDATE NO ACTION);

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
    ON UPDATE NO ACTION);

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
    ON UPDATE NO ACTION);

-- 5. Tabela ParadaRota
CREATE TABLE `arid_ponto`.`paradarota` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `Endereco` VARCHAR(250) NOT NULL,
  `Latitude` VARCHAR(50) NULL,
  `Longitude` VARCHAR(50) NULL,
  `Link` VARCHAR(500) NULL,
  `Entregue` TINYINT NOT NULL DEFAULT 0,
  `Observacao` VARCHAR(500) NULL,
  `ConcluidoEm` DATETIME NULL,
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
    ON UPDATE CASCADE);

-- 6. Tabela LocalizacaoRota
CREATE TABLE `arid_ponto`.`localizacaorota` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `Latitude` VARCHAR(50) NOT NULL,
  `Longitude` VARCHAR(50) NOT NULL,
  `DataHora` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_LocalizacaoRota_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_LocalizacaoRota_Rota_idx` (`RotaId` ASC) VISIBLE,
  CONSTRAINT `FK_LocalizacaoRota_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_LocalizacaoRota_Rota`
    FOREIGN KEY (`RotaId`)
    REFERENCES `arid_ponto`.`rota` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE);

-- Nota: Os Enumeradores de Permissão (eItemDePermissao_Motorista, eItemDePermissao_Veiculo, etc.)
-- são definidos em código (C#) e salvos na tabela `itemdogrupodepermissao` pelo sistema através 
-- da interface de controle de acesso de forma dinâmica.


-- 7. Tabela Motorista Historico Situação
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
    ON UPDATE NO ACTION);

-- 8. Tabela Rota Execução
CREATE TABLE `arid_ponto`.`rotaexecucao` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `DataHoraInicio` DATETIME NOT NULL,
  `DataHoraFim` DATETIME NULL,
  `UsuarioIdInicio` INT NOT NULL,
  `UsuarioIdFim` INT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_RotaExecucao_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_RotaExecucao_Rota_idx` (`RotaId` ASC) VISIBLE,
  INDEX `FK_RotaExecucao_UsuarioInicio_idx` (`UsuarioIdInicio` ASC) VISIBLE,
  INDEX `FK_RotaExecucao_UsuarioFim_idx` (`UsuarioIdFim` ASC) VISIBLE,
  CONSTRAINT `FK_RotaExecucao_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_RotaExecucao_Rota`
    FOREIGN KEY (`RotaId`)
    REFERENCES `arid_ponto`.`rota` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `FK_RotaExecucao_UsuarioInicio`
    FOREIGN KEY (`UsuarioIdInicio`)
    REFERENCES `arid_ponto`.`usuario` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_RotaExecucao_UsuarioFim`
    FOREIGN KEY (`UsuarioIdFim`)
    REFERENCES `arid_ponto`.`usuario` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
