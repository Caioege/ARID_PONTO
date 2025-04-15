ALTER TABLE `arid_escolas`.`aluno` 
ADD COLUMN `EscolaId` INT NULL AFTER `DataDeCadastro`,
ADD INDEX `FK_Aluno_Escola_idx` (`EscolaId` ASC);

ALTER TABLE `arid_escolas`.`aluno` 
ADD CONSTRAINT `FK_Aluno_Escola`
  FOREIGN KEY (`EscolaId`)
  REFERENCES `arid_escolas`.`escola` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
  CREATE TABLE `arid_escolas`.`alunoturma` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `RedeDeEnsinoid` INT NOT NULL,
  `TurmaId` INT NOT NULL,
  `AlunoId` INT NOT NULL,
  `EntradaNaTurma` DATETIME NOT NULL,
  `SaidaDaTurma` DATETIME NULL,
  `Situacao` INT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_AlunoTurma_RedeDeEnsino_idx` (`RedeDeEnsinoid` ASC),
  INDEX `FK_AlunoTurma_Turma_idx` (`TurmaId` ASC),
  INDEX `FK_AlunoTurma_Aluno_idx` (`AlunoId` ASC),
  CONSTRAINT `FK_AlunoTurma_RedeDeEnsino`
    FOREIGN KEY (`RedeDeEnsinoid`)
    REFERENCES `arid_escolas`.`rededeensino` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_AlunoTurma_Turma`
    FOREIGN KEY (`TurmaId`)
    REFERENCES `arid_escolas`.`turma` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_AlunoTurma_Aluno`
    FOREIGN KEY (`AlunoId`)
    REFERENCES `arid_escolas`.`aluno` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
    
ALTER TABLE `arid_escolas`.`aluno` 
ADD COLUMN `ConcluiuOsEstudos` TINYINT NULL DEFAULT 0;

ALTER TABLE `arid_escolas`.`aluno` 
ADD COLUMN `AnoEscolarAtual` INT NOT NULL DEFAULT 0;

ALTER TABLE `arid_escolas`.`aluno` 
ADD COLUMN `IdEquipamento` VARCHAR(12) NOT NULL;

CREATE TABLE `arid_escolas`.`itemhorariodeaula` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `RedeDeEnsinoId` INT NOT NULL,
  `TurmaId` INT NOT NULL,
  `DiaDaSemana` INT NOT NULL,
  `Disciplina` VARCHAR(100) NULL,
  `InicioAula` TIME NOT NULL,
  `FimAula` TIME NOT NULL,
  `Intervalo` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`),
  INDEX `FK_ItemHorarioDeAula_RedeDeEnsino_idx` (`RedeDeEnsinoId` ASC) VISIBLE,
  INDEX `FK_ItemHorarioDeAula_Turma_idx` (`TurmaId` ASC) VISIBLE,
  CONSTRAINT `FK_ItemHorarioDeAula_RedeDeEnsino`
    FOREIGN KEY (`RedeDeEnsinoId`)
    REFERENCES `arid_escolas`.`rededeensino` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_ItemHorarioDeAula_Turma`
    FOREIGN KEY (`TurmaId`)
    REFERENCES `arid_escolas`.`turma` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

ALTER TABLE `arid_escolas`.`turma` 
ADD COLUMN `DiasLetivos` INT NOT NULL DEFAULT 0;