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

ALTER TABLE `arid_ponto`.`pontododia` 
ADD COLUMN `HorasTrabalhadas` TIME NULL AFTER `CargaHoraria`,
ADD COLUMN `HorasTrabalhadasConsiderandoAbono` TIME NULL AFTER `HorasTrabalhadas`,
ADD COLUMN `HorasPositivas` TIME NULL AFTER `HorasTrabalhadasConsiderandoAbono`,
ADD COLUMN `HorasNegativas` TIME NULL AFTER `HorasPositivas`;

ALTER TABLE `arid_ponto`.`pontododia` 
ADD COLUMN `AfastamentoId` INT NULL AFTER `HorasNegativas`,
ADD INDEX `FK_PontoDoDia_Afastamento_idx` (`AfastamentoId` ASC);

ALTER TABLE `arid_ponto`.`pontododia` 
ADD CONSTRAINT `FK_PontoDoDia_Afastamento`
  FOREIGN KEY (`AfastamentoId`)
  REFERENCES `arid_ponto`.`afastamento` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
  CREATE TABLE `arid_ponto`.`equipamentodeponto` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `Descricao` VARCHAR(100) NOT NULL,
  `NumeroDeSerie` VARCHAR(100) NOT NULL,
  `Ativo` TINYINT NOT NULL DEFAULT 1,
  `UnidadeOrganizacionalId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_EquipamentoDePonto_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_EquipamentoDePonto_UnidadeOrganizacional_idx` (`UnidadeOrganizacionalId` ASC) VISIBLE,
  CONSTRAINT `FK_EquipamentoDePonto_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_EquipamentoDePonto_UnidadeOrganizacional`
    FOREIGN KEY (`UnidadeOrganizacionalId`)
    REFERENCES `arid_ponto`.`unidadeorganizacional` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

CREATE TABLE `arid_ponto`.`registrodeponto` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `EquipamentoDePontoId` INT NOT NULL,
  `UsuarioEquipamentoId` VARCHAR(50) NOT NULL,
  `TipoRegistro` INT NOT NULL,
  `DataHoraRegistro` DATETIME NOT NULL,
  `DataHoraRecebimento` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_RegistroDePonto_Organizacao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_RegistroDePonto_EquipamentoDePonto_idx` (`EquipamentoDePontoId` ASC) VISIBLE,
  CONSTRAINT `FK_RegistroDePonto_Organizacao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_RegistroDePonto_EquipamentoDePonto`
    FOREIGN KEY (`EquipamentoDePontoId`)
    REFERENCES `arid_ponto`.`equipamentodeponto` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
    
ALTER TABLE `arid_ponto`.`equipamentodeponto` 
ADD UNIQUE INDEX `UQ_EquipamentoDePonto` (`NumeroDeSerie` ASC, `OrganizacaoId` ASC);

CREATE TABLE `arid_ponto`.`grupodepermissao` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `Sigla` VARCHAR(5) NOT NULL,
  `Descricao` VARCHAR(100) NOT NULL,
  `PerfilDeAcesso` INT NOT NULL,
  `Ativo` TINYINT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Organizacao_GrupoDePermissao_idx` (`OrganizacaoId` ASC),
  CONSTRAINT `FK_Organizacao_GrupoDePermissao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
    
CREATE TABLE `arid_ponto`.`itemdogrupodepermissao` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `GrupoDePermissaoId` INT NOT NULL,
  `EnumeradorNome` VARCHAR(1000) NOT NULL,
  `ValorDoEnumerador` INT NOT NULL,
  `PermissaoAtiva` TINYINT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Organizacao_ItemDoGrupoDePermissao_idx` (`OrganizacaoId` ASC) VISIBLE,
  INDEX `FK_GrupoDePermissao_ItemDoGrupoDePermissao_idx` (`GrupoDePermissaoId` ASC) VISIBLE,
  CONSTRAINT `FK_Organizacao_ItemDoGrupoDePermissao`
    FOREIGN KEY (`OrganizacaoId`)
    REFERENCES `arid_ponto`.`organizacao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_GrupoDePermissao_ItemDoGrupoDePermissao`
    FOREIGN KEY (`GrupoDePermissaoId`)
    REFERENCES `arid_ponto`.`grupodepermissao` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
    
ALTER TABLE `arid_ponto`.`usuario` 
ADD COLUMN `GrupoDePermissaoId` INT NULL AFTER `NomeDaPessoa`,
ADD INDEX `FK_GrupoDePermissao_Usuario_idx` (`GrupoDePermissaoId` ASC) ;

ALTER TABLE `arid_ponto`.`usuario` 
ADD CONSTRAINT `FK_GrupoDePermissao_Usuario`
  FOREIGN KEY (`GrupoDePermissaoId`)
  REFERENCES `arid_ponto`.`grupodepermissao` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `arid_ponto`.`escaladoservidor` 
ADD UNIQUE INDEX `FK_UQ_EscalaServidor` (`OrganizacaoId` ASC, `EscalaId` ASC, `VinculoDeTrabalhoId` ASC, `Data` ASC);

ALTER TABLE `arid_ponto`.`escala` 
ADD COLUMN `UnidadeOrganizacionalId` INT NOT NULL AFTER `Descricao`,
ADD INDEX `FK_Unidade_Escala_idx` (`UnidadeOrganizacionalId` ASC);

ALTER TABLE `arid_ponto`.`escala` 
ADD CONSTRAINT `FK_Unidade_Escala`
  FOREIGN KEY (`UnidadeOrganizacionalId`)
  REFERENCES `arid_ponto`.`unidadeorganizacional` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
ALTER TABLE `arid_ponto`.`escaladoservidor` 
ADD COLUMN `DataFim` DATETIME NULL;

ALTER TABLE `arid_ponto`.`escaladoservidor` 
DROP FOREIGN KEY `FK_CicloDaEscala_EscalaDoServidor`;
ALTER TABLE `arid_ponto`.`escaladoservidor` 
CHANGE COLUMN `CicloDaEscalaId` `CicloDaEscalaId` INT NULL ;
ALTER TABLE `arid_ponto`.`escaladoservidor` 
ADD CONSTRAINT `FK_CicloDaEscala_EscalaDoServidor`
  FOREIGN KEY (`CicloDaEscalaId`)
  REFERENCES `arid_ponto`.`ciclodaescala` (`Id`);

-- ALTERACOES ROTA APP MObile N:N

ALTER TABLE `arid_ponto`.`rota`
DROP FOREIGN KEY `FK_Rota_Veiculo`;

ALTER TABLE `arid_ponto`.`rota`
DROP INDEX `FK_Rota_Veiculo_idx`;

ALTER TABLE `arid_ponto`.`rota`
DROP COLUMN `VeiculoId`;

CREATE TABLE `arid_ponto`.`rotaveiculo` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `OrganizacaoId` INT NOT NULL,
  `RotaId` INT NOT NULL,
  `VeiculoId` INT NOT NULL,
  PRIMARY KEY (`Id`),
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
    ON UPDATE NO ACTION);

ALTER TABLE `arid_ponto`.`rotaexecucao` 
ADD COLUMN `MotoristaId` INT NULL AFTER `RotaId`,
ADD COLUMN `VeiculoId` INT NULL AFTER `MotoristaId`,
ADD INDEX `FK_RotaExecucao_Motorista_idx` (`MotoristaId` ASC) VISIBLE,
ADD INDEX `FK_RotaExecucao_Veiculo_idx` (`VeiculoId` ASC) VISIBLE;

ALTER TABLE `arid_ponto`.`rotaexecucao` 
ADD CONSTRAINT `FK_RotaExecucao_Motorista`
  FOREIGN KEY (`MotoristaId`)
  REFERENCES `arid_ponto`.`motorista` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `FK_RotaExecucao_Veiculo`
  FOREIGN KEY (`VeiculoId`)
  REFERENCES `arid_ponto`.`veiculo` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
