ALTER TABLE RegistrosDePonto ADD COLUMN AprovadoForaTolerancia TINYINT(1) NULL;
ALTER TABLE RegistrosDePonto ADD COLUMN AcaoAprovacao VARCHAR(255) NULL;
ALTER TABLE RegistrosDePonto ADD COLUMN MotivoAprovacaoTolerancia VARCHAR(255) NULL;
ALTER TABLE RegistrosDePonto ADD COLUMN UsuarioAprovacaoToleranciaNome VARCHAR(255) NULL;
ALTER TABLE RegistrosDePonto ADD COLUMN DataAprovacaoTolerancia DATETIME NULL;
