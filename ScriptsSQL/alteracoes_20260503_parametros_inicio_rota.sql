ALTER TABLE rota
ADD COLUMN PermiteIniciarSemPacienteAcompanhante BIT(1) NOT NULL DEFAULT b'1',
ADD COLUMN PermiteIniciarSemProfissional BIT(1) NOT NULL DEFAULT b'1';
