-- Adicionar as informações do Transporte de Pacientes e Rotas não recorrentes
ALTER TABLE Rota ADD COLUMN DataParaExecucao DATETIME NULL;
ALTER TABLE Rota ADD COLUMN NomePaciente VARCHAR(150) NULL;
ALTER TABLE Rota ADD COLUMN MedicoResponsavel VARCHAR(200) NULL;

-- ---------------------------------------------------------------------------------------------------------
-- MOCK DATA: Histórico de Execução (ROTA ID: 7)
-- ---------------------------------------------------------------------------------------------------------

-- 1. Certifique-se que Usuário ID 1 exista ou mude os IDs conforme a sua base.
-- 2. Insere a Execução Finalizada a 1h atrás
INSERT INTO RotaExecucao 
(Inativo, DataInclusao, OrganizacaoId, RotaId, DataHoraInicio, DataHoraFim, UsuarioIdInicio, UsuarioIdFim) 
VALUES 
(0, NOW(), 7, 7, DATE_SUB(NOW(), INTERVAL 2 HOUR), DATE_SUB(NOW(), INTERVAL 30 MINUTE), 1, 1);

-- Captura o último id de execução inserido
SET @ExecId := LAST_INSERT_ID();

-- 3. Inserir Paradas (Exemplo de Clínicas/Hospitais) para simular paradas concluídas desta Rota 7
INSERT INTO paradarota
(Inativo, DataInclusao, OrganizacaoId, RotaId, Endereco, Link, Latitude, Longitude, Entregue, ConcluidoEm)
VALUES
(0, NOW(), 7, 7, 'Hospital de Base (Mock)', 'https://maps.google.com/?q=-15.793261,-47.883272', '-15.793261', '-47.883272', 1, DATE_SUB(NOW(), INTERVAL 85 MINUTE)),
(0, NOW(), 7, 7, 'Clínica Especializada (Mock)', 'https://maps.google.com/?q=-15.803924,-47.886843', '-15.803924', '-47.886843', 1, DATE_SUB(NOW(), INTERVAL 35 MINUTE));


-- 4. Insere Telemetria Simulada (Assumindo Rota de ID 7 com VeiculoID = 7)
-- Cuidado: Para que a query do monitoramento encontre a rota, associe a Rota 7 a um veículo que tenha esse id, ou mude o VeiculoId abaixo.
INSERT INTO LocalizacaoVeiculo 
(Inativo, DataInclusao, OrganizacaoId, VeiculoId, DataHora, Latitude, Longitude, Ignicao, Hodometro, Velocidade, Direcao, NivelSinal, Satelites, BateriaPrincipal, BateriaBackup, Bloqueio, Panico, Sirene)
VALUES 
(0, NOW(), 7, 7, DATE_SUB(NOW(), INTERVAL 115 MINUTE), '-15.794191', '-47.882130', 1, 1000, 45, 90, 100, 12, 12.5, 4.2, 0, 0, 0),
(0, NOW(), 7, 7, DATE_SUB(NOW(), INTERVAL 90 MINUTE), '-15.793261', '-47.883272', 1, 1005, 50, 95, 100, 12, 12.5, 4.2, 0, 0, 0),
(0, NOW(), 7, 7, DATE_SUB(NOW(), INTERVAL 60 MINUTE), '-15.796174', '-47.884074', 1, 1010, 40, 100, 100, 12, 12.5, 4.2, 0, 0, 0),
(0, NOW(), 7, 7, DATE_SUB(NOW(), INTERVAL 45 MINUTE), '-15.803924', '-47.886843', 1, 1015, 60, 110, 100, 12, 12.5, 4.2, 0, 0, 0),
(0, NOW(), 7, 7, DATE_SUB(NOW(), INTERVAL 30 MINUTE), '-15.815567', '-47.897940', 0, 1020, 0, 0, 100, 12, 12.5, 4.2, 0, 0, 0);
