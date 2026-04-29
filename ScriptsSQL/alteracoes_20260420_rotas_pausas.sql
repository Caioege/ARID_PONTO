-- Adicionar suporte de configuracao para pausas na rota base
ALTER TABLE `arid_ponto`.`rota`
ADD COLUMN `PermitePausa` BIT(1) NOT NULL DEFAULT b'0',
ADD COLUMN `QuantidadePausas` INT NOT NULL DEFAULT 0;
