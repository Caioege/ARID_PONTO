ALTER TABLE `arid_ponto`.`justificativadeausencia` 
ADD COLUMN `Ativa` INT NOT NULL DEFAULT 1 AFTER `LocalDeUso`;

CREATE TABLE `arid_ponto`.`afastamento` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `VinculoDeTrabalhoId` INT NOT NULL,
  `Inicio` DATETIME NOT NULL,
  `Fim` DATETIME NULL,
  `JustificativaDeAusenciaId` INT NOT NULL,
  `Observacao` VARCHAR(500) NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Afastamento_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_Afastamento_VinculoDeTrabalho_idx` (`VinculoDeTrabalhoId` ASC) VISIBLE,
  INDEX `FK_Afastamento_JustificativaDeAusencia_idx` (`JustificativaDeAusenciaId` ASC) VISIBLE,
  CONSTRAINT `FK_Afastamento_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Afastamento_VinculoDeTrabalho`
    FOREIGN KEY (`VinculoDeTrabalhoId`)
    REFERENCES `arid_ponto`.`vinculodetrabalho` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Afastamento_JustificativaDeAusencia`
    FOREIGN KEY (`JustificativaDeAusenciaId`)
    REFERENCES `arid_ponto`.`justificativadeausencia` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
    
ALTER TABLE `arid_ponto`.`pontododia` 
ADD COLUMN `TipoEntrada1` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoSaida1` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoEntrada2` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoSaida2` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoEntrada3` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoSaida3` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoEntrada4` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoSaida4` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoEntrada5` INT NOT NULL DEFAULT 0,
ADD COLUMN `TipoSaida5` INT NOT NULL DEFAULT 0;

CREATE TABLE `arid_ponto`.`eventoanual` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `Descricao` VARCHAR(150) NOT NULL,
  `Tipo` INT NOT NULL,
  `Data` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_EventoAnual_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  CONSTRAINT `FK_EventoAnual_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

ALTER TABLE `arid_ponto`.`pontododia` 
ADD COLUMN `PontoFechado` TINYINT NOT NULL DEFAULT 0 AFTER `TipoSaida5`,
ADD COLUMN `BancoDeHorasCredito` TIME NULL AFTER `PontoFechado`,
ADD COLUMN `BancoDeHorasDebito` TIME NULL AFTER `BancoDeHorasCredito`;

CREATE TABLE `arid_ponto`.`escala` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `Tipo` INT NOT NULL,
  `Descricao` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Organizacao_Escala_idx` (`OrganizacaoId` ASC) VISIBLE,
  CONSTRAINT `FK_Organizacao_Escala`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

CREATE TABLE `arid_ponto`.`ciclodaescala` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `EscalaId` INT NOT NULL,
  `Ciclo` INT NOT NULL,
  `Entrada1` TIME NULL,
  `Saida1` TIME NULL,
  `Entrada2` TIME NULL,
  `Saida2` TIME NULL,
  `Entrada3` TIME NULL,
  `Saida3` TIME NULL,
  `Entrada4` TIME NULL,
  `Saida4` TIME NULL,
  `Entrada5` TIME NULL,
  `Saida5` TIME NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Organizacao_CicloDaEscala_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_Escala_CicloDaEscala_idx` (`EscalaId` ASC) VISIBLE,
  CONSTRAINT `FK_Organizacao_CicloDaEscala`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Escala_CicloDaEscala`
    FOREIGN KEY (`EscalaId`)
    REFERENCES `arid_ponto`.`escala` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

CREATE TABLE `arid_ponto`.`escaladoservidor` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `EscalaId` INT NOT NULL,
  `CicloDaEscalaId` INT NOT NULL,
  `VinculoDeTrabalhoId` INT NOT NULL,
  `Data` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Organizacao_EscalaDoServidor_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_Escala_EscalaDoServidor_idx` (`EscalaId` ASC) VISIBLE,
  INDEX `FK_CicloDaEscala_EscalaDoServidor_idx` (`CicloDaEscalaId` ASC) VISIBLE,
  INDEX `FK_VinculoDeTrabalho_EscalaDoServidor_idx` (`VinculoDeTrabalhoId` ASC) VISIBLE,
  CONSTRAINT `FK_Organizacao_EscalaDoServidor`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_Escala_EscalaDoServidor`
    FOREIGN KEY (`EscalaId`)
    REFERENCES `arid_ponto`.`escala` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_CicloDaEscala_EscalaDoServidor`
    FOREIGN KEY (`CicloDaEscalaId`)
    REFERENCES `arid_ponto`.`ciclodaescala` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_VinculoDeTrabalho_EscalaDoServidor`
    FOREIGN KEY (`VinculoDeTrabalhoId`)
    REFERENCES `arid_ponto`.`vinculodetrabalho` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
    
    ALTER TABLE `arid_ponto`.`pontododia` 
ADD COLUMN `CargaHoraria` TIME NULL;

CREATE TABLE `arid_ponto`.`usuario` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NULL,
  `UsuarioDeAcesso` VARCHAR(100) NOT NULL,
  `UsuarioDeAcesso` VARCHAR(150) NOT NULL,
  `Senha` VARCHAR(1000) NOT NULL,
  `PerfilDeAcesso` INT NOT NULL,
  `Ativo` TINYINT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Organizacao_Usuario_idx` (`OrganizacaoId` ASC) VISIBLE,
  CONSTRAINT `FK_Organizacao_Usuario`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
