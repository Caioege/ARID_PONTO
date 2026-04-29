CREATE TABLE `arid_ponto`.`rotaveiculo` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `VeiculoId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `UK_RotaVeiculo_RotaVeiculo` (`RotaId`, `VeiculoId`),
  INDEX `FK_RotaVeiculo_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_RotaVeiculo_Rota_idx` (`RotaId` ASC) VISIBLE,
  INDEX `FK_RotaVeiculo_Veiculo_idx` (`VeiculoId` ASC) VISIBLE,
  CONSTRAINT `FK_RotaVeiculo_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_RotaVeiculo_Rota`
    FOREIGN KEY (`RotaId`)
    REFERENCES `arid_ponto`.`rota` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_RotaVeiculo_Veiculo`
    FOREIGN KEY (`VeiculoId`)
    REFERENCES `arid_ponto`.`veiculo` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
);

-- Adicionando Tipo de Veiculo para monitoramento diferenciado
ALTER TABLE `arid_ponto`.`veiculo`
ADD COLUMN `TipoVeiculo` INT NOT NULL DEFAULT 0;
