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
