ALTER TABLE rota ADD UnidadeOrigemId INT NULL;
ALTER TABLE rota ADD CONSTRAINT FK_Rota_UnidadeOrigem FOREIGN KEY (UnidadeOrigemId) REFERENCES unidade_organizacional(Id);
