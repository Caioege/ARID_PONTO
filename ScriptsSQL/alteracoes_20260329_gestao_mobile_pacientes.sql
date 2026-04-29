-- Adicionar informacoes do transporte de pacientes e rotas nao recorrentes
ALTER TABLE `arid_ponto`.`rota` ADD COLUMN `DataParaExecucao` DATETIME NULL;
ALTER TABLE `arid_ponto`.`rota` ADD COLUMN `NomePaciente` VARCHAR(150) NULL;
ALTER TABLE `arid_ponto`.`rota` ADD COLUMN `MedicoResponsavel` VARCHAR(200) NULL;
