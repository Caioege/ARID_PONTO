ALTER TABLE paradarota ADD UnidadeId INT NULL;
ALTER TABLE paradarota ADD CONSTRAINT FK_ParadaRota_Unidade FOREIGN KEY (UnidadeId) REFERENCES unidadeorganizacional(Id);
