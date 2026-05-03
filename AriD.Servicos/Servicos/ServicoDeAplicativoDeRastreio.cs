using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using Dapper;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeAplicativoDeRastreio : IServicoDeAplicativoDeRastreio
    {
        private const int StatusExecucaoEmAndamento = 1;
        private const int StatusExecucaoPausada = 2;
        private const int StatusExecucaoFinalizada = 3;

        private const int TipoEventoInicioRota = 1;
        private const int TipoEventoOrigem = 2;
        private const int TipoEventoParada = 3;
        private const int TipoEventoDestino = 4;
        private const int TipoEventoFimRota = 5;

        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<Motorista> _repositorioMotorista;
        private readonly IRepositorio<Rota> _repositorioRota;
        private readonly IRepositorio<RotaExecucao> _repositorioExecucao;
        private readonly IRepositorio<LocalizacaoRota> _repositorioLocalizacao;
        private readonly IRepositorio<ParadaRota> _repositorioParada;
        private readonly IRepositorio<ChecklistExecucao> _repositorioChecklistExec;
        private readonly IRepositorio<RotaExecucaoEvento> _repositorioEvento;
        private readonly IRepositorio<RotaExecucaoPausa> _repositorioPausa;
        private readonly IRepositorio<RotaExecucaoDesvio> _repositorioDesvio;

        public ServicoDeAplicativoDeRastreio(
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<Motorista> repositorioMotorista,
            IRepositorio<Rota> repositorioRota,
            IRepositorio<RotaExecucao> repositorioExecucao,
            IRepositorio<LocalizacaoRota> repositorioLocalizacao,
            IRepositorio<ParadaRota> repositorioParada,
            IRepositorio<ChecklistExecucao> repositorioChecklistExec,
            IRepositorio<RotaExecucaoEvento> repositorioEvento,
            IRepositorio<RotaExecucaoPausa> repositorioPausa,
            IRepositorio<RotaExecucaoDesvio> repositorioDesvio)
        {
            _repositorioServidor = repositorioServidor;
            _repositorioMotorista = repositorioMotorista;
            _repositorioRota = repositorioRota;
            _repositorioExecucao = repositorioExecucao;
            _repositorioLocalizacao = repositorioLocalizacao;
            _repositorioParada = repositorioParada;
            _repositorioChecklistExec = repositorioChecklistExec;
            _repositorioEvento = repositorioEvento;
            _repositorioPausa = repositorioPausa;
            _repositorioDesvio = repositorioDesvio;
        }

        public AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais)
        {
            bool isAcompanhante = credenciais.TipoAcesso?.ToLower() == "acompanhante";

            var queryAcesso =
                @"select
                    s.Id as ServidorId,
                    upper(p.Nome) as ServidorNome,
                    o.Id as OrganizacaoId,
                    o.Nome as OrganizacaoNome,
                    p.Cpf as Cpf,
                    s.RegistroDePontoNoAplicativo,
                    s.RegistroManualNoAplicativo,
                    s.RegistroDeAtestadoNoAplicativo,
                    s.TipoComprovacaoPontoApp as TipoComprovacaoApp,
                    p.DataDeNascimento,
                    s.Email,
                    m.NumeroCNH,
                    m.CategoriaCNH,
                    m.EmissaoCNH,
                    m.VencimentoCNH as ValidadeCNH
                from pessoa p
                inner join organizacao o on o.Id = p.OrganizacaoId
                inner join servidor s on s.PessoaId = p.Id
                left join motorista m on m.ServidorId = s.Id and m.Situacao = 0
                where replace(replace(p.Cpf, '.', ''), '-', '') = @USUARIO ";

            if (isAcompanhante)
            {
                queryAcesso += @" and exists (select 1 from rotaprofissional rp where rp.ServidorId = s.Id) ";
            }
            else
            {
                queryAcesso += @" and (IF(s.SenhaPersonalizadaDeAcesso IS NULL, DATE_FORMAT(p.DataDeNascimento, '%d%m%Y') = @SENHA, s.SenhaPersonalizadaDeAcesso = @SENHACRIPTOGRAFADA) = true)
                                  and m.Id IS NOT NULL ";
            }

            queryAcesso += @" limit 1";

            return _repositorioServidor.ConsultaDapper<AutenticacaoAppDTO>(queryAcesso, new
            {
                @USUARIO = ObterSomenteNumeros(credenciais.Usuario),
                @SENHA = ObterSomenteNumeros(credenciais.Senha),
                @SENHACRIPTOGRAFADA = Criptografia.CriptografarSenha(credenciais.Senha)
            }).FirstOrDefault();
        }

        public void RegistrarToken(int servidorId, string token, string plataforma)
        {
            var sql = "UPDATE servidor SET PushToken = @TOKEN, PlataformaDispositivo = @PLATAFORMA, UltimoAcessoApp = @AGORA WHERE Id = @ID";
            _repositorioServidor.ExecutarComando(sql, new { @TOKEN = token, @PLATAFORMA = plataforma, @AGORA = DateTime.Now, @ID = servidorId });
        }

        public List<RotaCheckListDTO> ObterRotasMotorista(int motoristaId)
        {
            var sql = @"
                SELECT r.Id, r.Descricao as Nome, r.Recorrente, r.DataParaExecucao, r.DataInicio, r.DataFim, r.DiasSemana
                FROM rota r
                WHERE r.Situacao = 1 AND (r.MotoristaId = @ID OR r.MotoristaSecundarioId = @ID)";

            var todas = _repositorioRota.ConsultaDapper<RotaAgendaAppDTO>(sql, new { @ID = motoristaId }).ToList();
            var hoje = DateTime.Now.Date;
            var flagHoje = ObterFlagHoje(hoje);

            return todas
                .Where(r =>
                {
                    bool isRecorrente = r.Recorrente;
                    if (!isRecorrente) return r.DataParaExecucao != null && r.DataParaExecucao.Value.Date == hoje;

                    if (r.DataInicio != null && r.DataInicio.Value.Date > hoje) return false;
                    if (r.DataFim != null && r.DataFim.Value.Date < hoje) return false;

                    if (r.DiasSemana.HasValue)
                    {
                        var rotadias = (eFlagDiaSemana)r.DiasSemana.Value;
                        return rotadias.HasFlag(flagHoje);
                    }
                    return false;
                })
                .Select(r => new RotaCheckListDTO
                {
                    Id = r.Id,
                    Codigo = $"RT-{r.Id:000}",
                    Nome = r.Nome,
                    Descricao = r.Recorrente ? $"{r.Nome} (Recorrente)" : $"{r.Nome} (Planejada para {r.DataParaExecucao:dd/MM/yyyy})"
                }).ToList();
        }

        public PacoteOfflineRastreioDTO ObterPacoteOfflineMotorista(int motoristaId)
        {
            var geradoEm = DateTime.Now;
            var validoAte = geradoEm.AddDays(3);

            var sql = @"
                SELECT
                    r.Id,
                    CONCAT('RT-', LPAD(r.Id, 3, '0')) as Codigo,
                    r.Descricao as Nome,
                    r.Descricao,
                    r.Recorrente,
                    r.DataParaExecucao,
                    r.DataInicio,
                    r.DataFim,
                    r.DiasSemana,
                    r.PermitePausa,
                    r.QuantidadePausas,
                    r.UnidadeOrigemId,
                    r.UnidadeDestinoId,
                    uo.Nome as NomeUnidadeOrigem,
                    ud.Nome as NomeUnidadeDestino,
                    uo.Latitude as OrigemLatitudeRota,
                    uo.Longitude as OrigemLongitudeRota,
                    ud.Latitude as DestinoLatitudeRota,
                    ud.Longitude as DestinoLongitudeRota
                FROM rota r
                LEFT JOIN unidadeorganizacional uo ON uo.Id = r.UnidadeOrigemId
                LEFT JOIN unidadeorganizacional ud ON ud.Id = r.UnidadeDestinoId
                WHERE r.Situacao = 1
                  AND (r.MotoristaId = @MotoristaId OR r.MotoristaSecundarioId = @MotoristaId)";

            var rotasBase = _repositorioRota.ConsultaDapper<RotaOfflineConsultaDTO>(sql, new { @MotoristaId = motoristaId }).ToList();
            var rotasValidas = rotasBase.Where(r => RotaPodeExecutarNoPeriodo(r, geradoEm.Date, validoAte.Date)).ToList();

            var pacote = new PacoteOfflineRastreioDTO
            {
                DataHoraGeracao = geradoEm,
                ValidoAte = validoAte,
                ValidadeEmDias = 3
            };

            foreach (var rota in rotasValidas)
            {
                pacote.Rotas.Add(new RotaOfflineDTO
                {
                    Id = rota.Id,
                    Codigo = rota.Codigo,
                    Nome = rota.Nome,
                    Descricao = rota.Recorrente ? $"{rota.Descricao} (Recorrente)" : $"{rota.Descricao} (Planejada para {rota.DataParaExecucao:dd/MM/yyyy})",
                    PermitePausa = Convert.ToBoolean(rota.PermitePausa),
                    QuantidadePausas = Convert.ToInt32(rota.QuantidadePausas),
                    UnidadeOrigemId = rota.UnidadeOrigemId,
                    UnidadeDestinoId = rota.UnidadeDestinoId,
                    NomeUnidadeOrigem = rota.NomeUnidadeOrigem,
                    NomeUnidadeDestino = rota.NomeUnidadeDestino,
                    OrigemLatitudeRota = rota.OrigemLatitudeRota,
                    OrigemLongitudeRota = rota.OrigemLongitudeRota,
                    DestinoLatitudeRota = rota.DestinoLatitudeRota,
                    DestinoLongitudeRota = rota.DestinoLongitudeRota,
                    Veiculos = ObterVeiculosChecklist(rota.Id),
                    Paradas = ObterParadasOffline(rota.Id)
                });
            }

            return pacote;
        }

        public ResultadoSincronizacaoRotaOfflineDTO SincronizarRotaOffline(SincronizarRotaOfflineDTO dto, int motoristaId)
        {
            if (dto == null)
                throw new ApplicationException("Payload de sincronizacao offline nao informado.");

            if (string.IsNullOrWhiteSpace(dto.LocalExecucaoId))
                throw new ApplicationException("LocalExecucaoId nao informado.");

            var agora = DateTime.Now;
            dto.IdentificadorDispositivo = string.IsNullOrWhiteSpace(dto.IdentificadorDispositivo)
                ? null
                : dto.IdentificadorDispositivo.Trim();

            using var conn = _repositorioExecucao.MySQLConn();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var rota = conn.QueryFirstOrDefault<RotaSincronizacaoOfflineLookupDTO>(
                    @"SELECT Id, OrganizacaoId, UnidadeOrigemId, UnidadeDestinoId
                      FROM rota
                      WHERE Id = @RotaId",
                    new { dto.RotaId },
                    transaction);

                if (rota == null)
                    throw new ApplicationException("Rota informada na sincronizacao offline nao existe.");

                var execucaoId = conn.QueryFirstOrDefault<int?>(
                    "SELECT Id FROM rotaexecucao WHERE LocalExecucaoId = @LocalExecucaoId LIMIT 1",
                    new { dto.LocalExecucaoId },
                    transaction);

                if (!execucaoId.HasValue)
                {
                    var execucaoAtiva = conn.QueryFirstOrDefault<int?>(
                        @"SELECT Id
                          FROM rotaexecucao
                          WHERE MotoristaId = @MotoristaId
                            AND Status IN (@StatusExecucaoEmAndamento, @StatusExecucaoPausada)
                            AND COALESCE(LocalExecucaoId, '') <> @LocalExecucaoId
                          LIMIT 1",
                        new
                        {
                            MotoristaId = motoristaId,
                            StatusExecucaoEmAndamento,
                            StatusExecucaoPausada,
                            dto.LocalExecucaoId
                        },
                        transaction);

                    if (execucaoAtiva.HasValue)
                        throw new ApplicationException("O motorista possui outra rota em andamento no servidor. A sincronizacao automatica foi bloqueada para auditoria.");

                    var checklistExecucaoId = CriarChecklistOfflineSeNecessario(conn, transaction, dto, motoristaId, rota.OrganizacaoId, agora);
                    var statusInicial = dto.DataHoraFimLocal.HasValue ? StatusExecucaoFinalizada : StatusExecucaoEmAndamento;

                    execucaoId = conn.QuerySingle<int>(
                        @"INSERT INTO rotaexecucao
                            (OrganizacaoId, RotaId, MotoristaId, VeiculoId, ChecklistExecucaoId, Status,
                             DataHoraInicio, DataHoraFim, UsuarioIdInicio, UsuarioIdFim, ObservacaoInicio, ObservacaoFim,
                             UltimaLatitude, UltimaLongitude, UltimaAtualizacaoEm, GpsSimuladoUltimaLeitura,
                             PossuiRegistroOffline, ExecucaoOfflineCompleta, DataHoraPrimeiroRegistroOffline,
                             DataHoraUltimoRegistroOffline, DataHoraUltimaComunicacaoApp, LocalExecucaoId,
                             IdentificadorDispositivo, DataCriacao, DataAlteracao)
                          VALUES
                            (@OrganizacaoId, @RotaId, @MotoristaId, @VeiculoId, @ChecklistExecucaoId, @Status,
                             @DataHoraInicio, @DataHoraFim, NULL, NULL, @ObservacaoInicio, @ObservacaoFim,
                             @UltimaLatitude, @UltimaLongitude, @UltimaAtualizacaoEm, @GpsSimulado,
                             1, @ExecucaoOfflineCompleta, @PrimeiroOffline, @UltimoOffline, @Comunicacao,
                             @LocalExecucaoId, @IdentificadorDispositivo, @Agora, @Agora);
                          SELECT LAST_INSERT_ID();",
                        new
                        {
                            rota.OrganizacaoId,
                            dto.RotaId,
                            MotoristaId = motoristaId,
                            dto.VeiculoId,
                            ChecklistExecucaoId = checklistExecucaoId,
                            Status = statusInicial,
                            DataHoraInicio = dto.DataHoraInicioLocal,
                            DataHoraFim = dto.DataHoraFimLocal,
                            dto.ObservacaoInicio,
                            dto.ObservacaoFim,
                            UltimaLatitude = ObterUltimaLatitude(dto),
                            UltimaLongitude = ObterUltimaLongitude(dto),
                            UltimaAtualizacaoEm = ObterUltimaDataHora(dto),
                            GpsSimulado = ObterUltimoGpsSimulado(dto),
                            ExecucaoOfflineCompleta = dto.DataHoraFimLocal.HasValue,
                            PrimeiroOffline = ObterPrimeiroRegistroOffline(dto),
                            UltimoOffline = ObterUltimoRegistroOffline(dto),
                            Comunicacao = agora,
                            dto.LocalExecucaoId,
                            dto.IdentificadorDispositivo,
                            Agora = agora
                        },
                        transaction);

                    RegistrarSincronizacaoOffline(conn, transaction, rota.OrganizacaoId, execucaoId.Value, dto.LocalExecucaoId, $"{dto.LocalExecucaoId}_inicio", "inicio_rota", dto.IdentificadorDispositivo, dto.DataHoraInicioLocal, agora);
                }
                else
                {
                    conn.Execute(
                        @"UPDATE rotaexecucao
                          SET DataHoraUltimaComunicacaoApp = @Agora,
                              IdentificadorDispositivo = COALESCE(IdentificadorDispositivo, @IdentificadorDispositivo),
                              DataAlteracao = @Agora
                          WHERE Id = @ExecucaoId",
                        new { Agora = agora, dto.IdentificadorDispositivo, ExecucaoId = execucaoId.Value },
                        transaction);
                }

                SalvarEventosOffline(conn, transaction, dto, execucaoId.Value, rota, agora);
                SalvarLocalizacoesOffline(conn, transaction, dto, execucaoId.Value, rota.OrganizacaoId, motoristaId, agora);
                SalvarPausasOffline(conn, transaction, dto, execucaoId.Value, rota.OrganizacaoId, agora);
                AtualizarResumoOfflineExecucao(conn, transaction, dto, execucaoId.Value, agora);

                transaction.Commit();

                return new ResultadoSincronizacaoRotaOfflineDTO
                {
                    Sucesso = true,
                    RotaExecucaoId = execucaoId.Value,
                    LocalExecucaoId = dto.LocalExecucaoId,
                    Mensagem = "Sincronizacao offline recebida com sucesso.",
                    DataHoraSincronizacao = agora
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<RotaCheckListDTO> ObterRotasAcompanhante(int servidorId)
        {
            var sql = @"
                SELECT r.Id, r.Descricao as Nome, r.Recorrente, r.DataParaExecucao, r.DataInicio, r.DataFim, r.DiasSemana
                FROM rota r
                INNER JOIN rotaprofissional rp ON rp.RotaId = r.Id
                WHERE r.Situacao = 1 AND rp.ServidorId = @ID";

            var todas = _repositorioRota.ConsultaDapper<RotaAgendaAppDTO>(sql, new { @ID = servidorId }).ToList();
            var hoje = DateTime.Now.Date;
            var flagHoje = ObterFlagHoje(hoje);

            return todas
                .Where(r =>
                {
                    bool isRecorrente = r.Recorrente;
                    if (!isRecorrente) return r.DataParaExecucao != null && r.DataParaExecucao.Value.Date == hoje;

                    if (r.DataInicio != null && r.DataInicio.Value.Date > hoje) return false;
                    if (r.DataFim != null && r.DataFim.Value.Date < hoje) return false;

                    if (r.DiasSemana.HasValue)
                    {
                        var rotadias = (eFlagDiaSemana)r.DiasSemana.Value;
                        return rotadias.HasFlag(flagHoje);
                    }
                    return false;
                })
                .Select(r => new RotaCheckListDTO
                {
                    Id = r.Id,
                    Codigo = $"RT-{r.Id:000}",
                    Nome = r.Nome,
                    Descricao = r.Recorrente ? $"{r.Nome} (Recorrente)" : $"{r.Nome} (Planejada para {r.DataParaExecucao:dd/MM/yyyy})"
                }).ToList();
        }

        public UltimaLocalizacaoRotaDTO? ObterUltimaLocalizacao(int rotaId)
        {
            var sql = @"
                SELECT l.Latitude, l.Longitude, l.DataHoraCaptura as DataHora
                FROM rotaexecucao re
                INNER JOIN rotaexecucaolocalizacao l ON l.RotaExecucaoId = re.Id
                WHERE re.RotaId = @ID
                ORDER BY l.DataHoraCaptura DESC, l.Id DESC
                LIMIT 1";
            return _repositorioLocalizacao.ConsultaDapper<UltimaLocalizacaoRotaDTO>(sql, new { @ID = rotaId }).FirstOrDefault();
        }

        public IEnumerable<UltimaLocalizacaoRotaDTO> ObterTrajeto(int rotaId, DateTime data)
        {
            var sql = @"
                SELECT l.Latitude, l.Longitude, l.DataHoraCaptura as DataHora
                FROM rotaexecucao re
                INNER JOIN rotaexecucaolocalizacao l ON l.RotaExecucaoId = re.Id
                WHERE re.RotaId = @ID AND DATE(l.DataHoraCaptura) = @DATA
                ORDER BY l.DataHoraCaptura";
            return _repositorioLocalizacao.ConsultaDapper<UltimaLocalizacaoRotaDTO>(sql, new { @ID = rotaId, @DATA = data.Date });
        }

        public List<VeiculoCheckListDTO> ObterVeiculosChecklist(int rotaId)
        {
            var sqlVeiculos = @"
                SELECT v.Id, v.Modelo, v.Placa, v.Cor
                FROM veiculo v
                INNER JOIN rotaveiculo rv ON rv.VeiculoId = v.Id
                WHERE rv.RotaId = @RotaId";

            var veiculos = _repositorioRota.ConsultaDapper<VeiculoResumoAppDTO>(sqlVeiculos, new { @RotaId = rotaId }).ToList();

            var resultado = new List<VeiculoCheckListDTO>();
            foreach (var v in veiculos)
            {
                var sqlItems = "SELECT Id, Descricao FROM checklistitem WHERE VeiculoId = @VeiculoId AND Ativo = 1";
                var items = _repositorioRota.ConsultaDapper<CheckListItemDTO>(sqlItems, new { @VeiculoId = v.Id }).ToList();

                resultado.Add(new VeiculoCheckListDTO
                {
                    Id = v.Id,
                    RotaId = rotaId,
                    Nome = v.Modelo,
                    Placa = v.Placa,
                    Modelo = v.Modelo,
                    Cor = v.Cor.ToString(),
                    Checklist = items
                });
            }
            return resultado;
        }

        public int SalvarChecklist(ChecklistPostDTO dto, int motoristaId)
        {
            var sqlChecklist = @"INSERT INTO checklistexecucao (OrganizacaoId, VeiculoId, MotoristaId, RotaId, DataHora)
                                 VALUES ((SELECT OrganizacaoId FROM veiculo WHERE Id = @VeiculoId), @VeiculoId, @MotoristaId, @RotaId, @Agora);
                                 SELECT LAST_INSERT_ID();";

            var id = _repositorioChecklistExec.ConsultaDapper<int>(sqlChecklist, new
            {
                @VeiculoId = dto.VeiculoId,
                @MotoristaId = motoristaId,
                @RotaId = dto.RotaId,
                @Agora = DateTime.Now
            }).First();

            foreach (var itemId in dto.Itens)
            {
                var sqlItem = "INSERT INTO checklistexecucaoitem (ChecklistExecucaoId, ChecklistItemId, Marcado) VALUES (@ExecId, @ItemId, 1)";
                _repositorioChecklistExec.ExecutarComando(sqlItem, new { @ExecId = id, @ItemId = itemId });
            }

            return id;
        }

        public RotaExecucaoDTO IniciarRota(IniciarRotaAppDTO dto, int motoristaId)
        {
            if (!dto.ChecklistExecucaoId.HasValue)
                throw new ApplicationException("O checklist deve ser salvo antes de iniciar a rota.");

            ValidarChecklistParaInicio(dto, motoristaId);

            var sqlExecucaoAtivaMotorista = "SELECT Id FROM rotaexecucao WHERE MotoristaId = @MotoristaId AND Status IN (1, 2) LIMIT 1";
            if (_repositorioExecucao.ConsultaDapper<int>(sqlExecucaoAtivaMotorista, new { @MotoristaId = motoristaId }).Any())
                throw new ApplicationException("O motorista ja possui uma rota em andamento.");

            var sqlExecucaoAtivaRota = "SELECT Id FROM rotaexecucao WHERE RotaId = @RotaId AND Status IN (1, 2) LIMIT 1";
            if (_repositorioExecucao.ConsultaDapper<int>(sqlExecucaoAtivaRota, new { @RotaId = dto.RotaId }).Any())
                throw new ApplicationException("Esta rota ja esta em execucao.");

            var sqlInsert = @"INSERT INTO rotaexecucao
                                (OrganizacaoId, RotaId, MotoristaId, VeiculoId, ChecklistExecucaoId, Status, DataHoraInicio, UsuarioIdInicio, ObservacaoInicio, UltimaLatitude, UltimaLongitude, UltimaAtualizacaoEm, GpsSimuladoUltimaLeitura, DataHoraUltimaComunicacaoApp)
                              VALUES
                                ((SELECT OrganizacaoId FROM rota WHERE Id = @RotaId), @RotaId, @MotoristaId, @VeiculoId, @ChecklistId, @Status, @Agora, NULL, @ObsInicio, @Lat, @Lon, @Agora, @GpsSimulado, @Agora);
                              SELECT LAST_INSERT_ID();";

            var agora = DateTime.Now;
            var execucaoId = _repositorioExecucao.ConsultaDapper<int>(sqlInsert, new
            {
                @RotaId = dto.RotaId,
                @MotoristaId = motoristaId,
                @VeiculoId = dto.VeiculoId,
                @ChecklistId = dto.ChecklistExecucaoId,
                @Status = StatusExecucaoEmAndamento,
                @Agora = agora,
                @ObsInicio = dto.ObservacaoInicio,
                @Lat = dto.LatitudeInicio,
                @Lon = dto.LongitudeInicio,
                @GpsSimulado = dto.GpsSimulado
            }).First();

            SalvarEventoExecucao(execucaoId, TipoEventoInicioRota, null, null, true, dto.ObservacaoInicio, dto.LatitudeInicio, dto.LongitudeInicio, dto.GpsSimulado, agora);

            if (!string.IsNullOrWhiteSpace(dto.LatitudeInicio) && !string.IsNullOrWhiteSpace(dto.LongitudeInicio))
            {
                SalvarPontoInterno(new PostLocalizacaoExecucaoDTO
                {
                    RotaExecucaoId = execucaoId,
                    Latitude = dto.LatitudeInicio,
                    Longitude = dto.LongitudeInicio,
                    DataHora = agora,
                    GpsSimulado = dto.GpsSimulado,
                    FonteCaptura = 2
                }, motoristaId, validarStatusAtivo: false);
            }

            return ObterRotaEmAndamentoAux(execucaoId);
        }

        public RotaExecucaoDTO? ObterRotaEmAndamento(int motoristaId)
        {
            var sqlAtiva = "SELECT Id FROM rotaexecucao WHERE MotoristaId = @MotoristaId AND Status IN (1, 2) ORDER BY DataHoraInicio DESC LIMIT 1";
            var execucaoId = _repositorioExecucao.ConsultaDapper<int>(sqlAtiva, new { @MotoristaId = motoristaId }).FirstOrDefault();

            if (execucaoId == 0) return null;

            return ObterRotaEmAndamentoAux(execucaoId);
        }

        private RotaExecucaoDTO ObterRotaEmAndamentoAux(int execucaoId)
        {
            var sqlExecucao = @"
                SELECT e.Id, e.RotaId, e.Status, e.VeiculoId, e.ChecklistExecucaoId, e.DataHoraInicio,
                       r.Descricao, r.PermitePausa, r.QuantidadePausas,
                       r.UnidadeOrigemId, r.UnidadeDestinoId,
                       uo.Nome as NomeUnidadeOrigem, ud.Nome as NomeUnidadeDestino,
                       uo.Latitude as OrigemLatitudeRota, uo.Longitude as OrigemLongitudeRota,
                       ud.Latitude as DestinoLatitudeRota, ud.Longitude as DestinoLongitudeRota
                FROM rotaexecucao e
                INNER JOIN rota r ON r.Id = e.RotaId
                LEFT JOIN unidadeorganizacional uo ON uo.Id = r.UnidadeOrigemId
                LEFT JOIN unidadeorganizacional ud ON ud.Id = r.UnidadeDestinoId
                WHERE e.Id = @ExecId";

            var execucao = _repositorioExecucao.ConsultaDapper<RotaExecucaoResumoDTO>(sqlExecucao, new { @ExecId = execucaoId }).First();

            var sqlOrigem = @"
                SELECT Entregue, Observacao, Latitude, Longitude, DataHoraEvento
                FROM rotaexecucaoevento
                WHERE RotaExecucaoId = @ExecId AND TipoEvento = @TipoEvento
                ORDER BY DataHoraEvento DESC, Id DESC
                LIMIT 1";
            var origem = _repositorioEvento.ConsultaDapper<RotaExecucaoEventoResumoDTO>(sqlOrigem, new { @ExecId = execucaoId, @TipoEvento = TipoEventoOrigem }).FirstOrDefault();
            var destino = _repositorioEvento.ConsultaDapper<RotaExecucaoEventoResumoDTO>(sqlOrigem, new { @ExecId = execucaoId, @TipoEvento = TipoEventoDestino }).FirstOrDefault();

            var campoObservacaoCadastro = ColunaExiste("paradarota", "ObservacaoCadastro")
                ? "p.ObservacaoCadastro"
                : "NULL as ObservacaoCadastro";

            var sqlParadas = $@"
                SELECT
                    p.Id,
                    p.Endereco,
                    p.Latitude,
                    p.Longitude,
                    p.Link,
                    {campoObservacaoCadastro},
                    ev.Entregue,
                    ev.Observacao,
                    ev.DataHoraEvento as ConcluidoEm,
                    ev.Latitude as LatitudeConfirmacao,
                    ev.Longitude as LongitudeConfirmacao
                FROM paradarota p
                LEFT JOIN rotaexecucaoevento ev ON ev.Id = (
                    SELECT ev2.Id
                    FROM rotaexecucaoevento ev2
                    WHERE ev2.RotaExecucaoId = @ExecId
                      AND ev2.TipoEvento = @TipoEventoParada
                      AND ev2.ParadaRotaId = p.Id
                    ORDER BY ev2.DataHoraEvento DESC, ev2.Id DESC
                    LIMIT 1
                )
                WHERE p.RotaId = @RotaId
                ORDER BY p.Ordem ASC, p.Id ASC";

            var paradas = _repositorioParada.ConsultaDapper<ParadaRotaDTO>(sqlParadas, new
            {
                @ExecId = execucaoId,
                @RotaId = (int)execucao.RotaId,
                @TipoEventoParada = TipoEventoParada
            }).ToList();

            var sqlPausas = @"
                SELECT COUNT(1)
                FROM rotaexecucaopausa
                WHERE RotaExecucaoId = @ExecId";
            var quantidadePausas = _repositorioPausa.ConsultaDapper<int>(sqlPausas, new { @ExecId = execucaoId }).FirstOrDefault();

            var sqlPausaAberta = @"
                SELECT COUNT(1)
                FROM rotaexecucaopausa
                WHERE RotaExecucaoId = @ExecId AND DataHoraFim IS NULL";
            var estaPausada = _repositorioPausa.ConsultaDapper<int>(sqlPausaAberta, new { @ExecId = execucaoId }).FirstOrDefault() > 0;

            return new RotaExecucaoDTO
            {
                Id = execucaoId,
                RotaId = execucao.RotaId,
                Descricao = execucao.Descricao,
                EmAndamento = (int)execucao.Status is StatusExecucaoEmAndamento or StatusExecucaoPausada,
                PermitePausa = Convert.ToBoolean(execucao.PermitePausa),
                QuantidadePausas = Convert.ToInt32(execucao.QuantidadePausas),
                QuantidadePausasRealizadas = quantidadePausas,
                EstaPausada = estaPausada,
                NomeUnidadeOrigem = execucao.NomeUnidadeOrigem,
                OrigemLatitudeRota = execucao.OrigemLatitudeRota,
                OrigemLongitudeRota = execucao.OrigemLongitudeRota,
                OrigemEntregue = origem?.Entregue == null ? null : Convert.ToBoolean(origem.Entregue),
                OrigemObservacao = origem?.Observacao,
                OrigemConcluidaEm = origem?.DataHoraEvento != null ? ((DateTime)origem.DataHoraEvento).ToString("yyyy-MM-ddTHH:mm:ss") : null,
                OrigemLatitude = origem?.Latitude,
                OrigemLongitude = origem?.Longitude,
                NomeUnidadeDestino = execucao.NomeUnidadeDestino,
                DestinoLatitudeRota = execucao.DestinoLatitudeRota,
                DestinoLongitudeRota = execucao.DestinoLongitudeRota,
                DestinoEntregue = destino?.Entregue == null ? null : Convert.ToBoolean(destino.Entregue),
                DestinoObservacao = destino?.Observacao,
                DestinoConcluidoEm = destino?.DataHoraEvento != null ? ((DateTime)destino.DataHoraEvento).ToString("yyyy-MM-ddTHH:mm:ss") : null,
                DestinoLatitude = destino?.Latitude,
                DestinoLongitude = destino?.Longitude,
                VeiculoId = execucao.VeiculoId,
                ChecklistExecucaoId = execucao.ChecklistExecucaoId,
                Paradas = paradas
            };
        }

        public void EncerrarRota(EncerrarRotaAppDTO dto, int motoristaId)
        {
            var execucao = ObterExecucaoValida(dto.RotaExecucaoId, motoristaId);
            if (execucao.Status == StatusExecucaoFinalizada)
                throw new ApplicationException("A rota ja foi finalizada.");

            if (execucao.Status == StatusExecucaoPausada)
                throw new ApplicationException("Nao e possivel encerrar a rota enquanto ela estiver pausada.");

            var agora = DateTime.Now;
            SalvarEventoExecucao(dto.RotaExecucaoId, TipoEventoFimRota, null, null, true, dto.Observacao, execucao.UltimaLatitude, execucao.UltimaLongitude, execucao.GpsSimuladoUltimaLeitura, agora);

            var sql = @"UPDATE rotaexecucao
                        SET Status = @StatusFinalizada,
                            DataHoraFim = @Agora,
                            ObservacaoFim = @Obs,
                            DataHoraUltimaComunicacaoApp = @Agora,
                            DataAlteracao = @Agora
                        WHERE Id = @Id";
            _repositorioExecucao.ExecutarComando(sql, new
            {
                @StatusFinalizada = StatusExecucaoFinalizada,
                @Agora = agora,
                @Obs = dto.Observacao,
                @Id = dto.RotaExecucaoId
            });
        }

        public void ConfirmarParada(ConfirmarParadaAppDTO dto, int motoristaId)
        {
            var execucao = ObterExecucaoValida(dto.RotaExecucaoId, motoristaId, validarAtiva: true);

            int tipoEvento;
            int? paradaRotaId = null;
            int? unidadeId = null;
            if (dto.ParadaId == -1)
            {
                tipoEvento = TipoEventoOrigem;
                unidadeId = execucao.UnidadeOrigemId;
            }
            else if (dto.ParadaId == -2)
            {
                tipoEvento = TipoEventoDestino;
                unidadeId = execucao.UnidadeDestinoId;
            }
            else
            {
                tipoEvento = TipoEventoParada;
                paradaRotaId = dto.ParadaId;
            }

            SalvarEventoExecucao(dto.RotaExecucaoId, tipoEvento, paradaRotaId, unidadeId, dto.Entregue, dto.Observacao, dto.Latitude, dto.Longitude, execucao.GpsSimuladoUltimaLeitura, DateTime.Now);
        }

        public void SalvarPonto(PostLocalizacaoExecucaoDTO dto, int motoristaId)
        {
            SalvarPontoInterno(dto, motoristaId, validarStatusAtivo: true);
        }

        public void FazerPausa(PausaRotaAppDTO dto, int motoristaId)
        {
            var execucao = ObterExecucaoValida(dto.RotaExecucaoId, motoristaId, validarAtiva: true);
            if (execucao.Status == StatusExecucaoPausada)
                throw new ApplicationException("Ja existe uma pausa em andamento.");

            var sqlPermissaoPausa = @"
                SELECT r.PermitePausa, r.QuantidadePausas
                FROM rotaexecucao e
                INNER JOIN rota r ON r.Id = e.RotaId
                WHERE e.Id = @ExecId";
            var dadosPausa = _repositorioExecucao.ConsultaDapper<RotaExecucaoPermissaoPausaDTO>(sqlPermissaoPausa, new { @ExecId = dto.RotaExecucaoId }).First();

            if (!Convert.ToBoolean(dadosPausa.PermitePausa))
                throw new ApplicationException("A rota nao permite pausas.");

            var quantidadeRealizada = _repositorioPausa.ConsultaDapper<int>("SELECT COUNT(1) FROM rotaexecucaopausa WHERE RotaExecucaoId = @ExecId", new { @ExecId = dto.RotaExecucaoId }).FirstOrDefault();
            if (quantidadeRealizada >= Convert.ToInt32(dadosPausa.QuantidadePausas))
                throw new ApplicationException("Limite de pausas atingido para esta rota.");

            var sqlInsert = @"
                INSERT INTO rotaexecucaopausa
                    (OrganizacaoId, RotaExecucaoId, Motivo, DataHoraInicio, LatitudeInicio, LongitudeInicio, GpsSimuladoInicio, UsuarioIdRegistro)
                VALUES
                    (@OrgId, @ExecId, @Motivo, @DataHoraInicio, @LatitudeInicio, @LongitudeInicio, @GpsSimuladoInicio, NULL)";
            _repositorioPausa.ExecutarComando(sqlInsert, new
            {
                @OrgId = execucao.OrganizacaoId,
                @ExecId = dto.RotaExecucaoId,
                @Motivo = dto.Motivo ?? "Pausa informada pelo app",
                @DataHoraInicio = dto.DataHora,
                @LatitudeInicio = dto.Latitude,
                @LongitudeInicio = dto.Longitude,
                @GpsSimuladoInicio = execucao.GpsSimuladoUltimaLeitura
            });

            _repositorioExecucao.ExecutarComando("UPDATE rotaexecucao SET Status = @Status, DataHoraUltimaComunicacaoApp = @Agora, DataAlteracao = @Agora WHERE Id = @Id", new
            {
                @Status = StatusExecucaoPausada,
                @Agora = DateTime.Now,
                @Id = dto.RotaExecucaoId
            });
        }

        public void FinalizarPausa(PausaRotaAppDTO dto, int motoristaId)
        {
            var execucao = ObterExecucaoValida(dto.RotaExecucaoId, motoristaId);
            if (execucao.Status != StatusExecucaoPausada)
                throw new ApplicationException("Nao existe pausa em andamento para esta rota.");

            var sqlPausaAberta = @"
                SELECT Id
                FROM rotaexecucaopausa
                WHERE RotaExecucaoId = @ExecId AND DataHoraFim IS NULL
                ORDER BY DataHoraInicio DESC
                LIMIT 1";
            var pausaId = _repositorioPausa.ConsultaDapper<long>(sqlPausaAberta, new { @ExecId = dto.RotaExecucaoId }).FirstOrDefault();
            if (pausaId == 0)
                throw new ApplicationException("Nao existe pausa em andamento para esta rota.");

            var sqlUpdate = @"
                UPDATE rotaexecucaopausa
                SET DataHoraFim = @DataHoraFim,
                    LatitudeFim = @LatitudeFim,
                    LongitudeFim = @LongitudeFim,
                    GpsSimuladoFim = @GpsSimuladoFim
                WHERE Id = @Id";
            _repositorioPausa.ExecutarComando(sqlUpdate, new
            {
                @DataHoraFim = dto.DataHora,
                @LatitudeFim = dto.Latitude,
                @LongitudeFim = dto.Longitude,
                @GpsSimuladoFim = execucao.GpsSimuladoUltimaLeitura,
                @Id = pausaId
            });

            _repositorioExecucao.ExecutarComando("UPDATE rotaexecucao SET Status = @Status, DataHoraUltimaComunicacaoApp = @Agora, DataAlteracao = @Agora WHERE Id = @Id", new
            {
                @Status = StatusExecucaoEmAndamento,
                @Agora = DateTime.Now,
                @Id = dto.RotaExecucaoId
            });
        }

        public void ReceberLocalizacao(PostLocalizacaoRotaDTO dto)
        {
            var sqlExecucao = @"
                SELECT Id, MotoristaId
                FROM rotaexecucao
                WHERE RotaId = @RotaId AND Status IN (1, 2)
                ORDER BY DataHoraInicio DESC
                LIMIT 1";
            var execucao = _repositorioExecucao.ConsultaDapper<RotaExecucaoLookupDTO>(sqlExecucao, new { @RotaId = dto.RotaId }).FirstOrDefault();
            if (execucao == null) return;

            SalvarPontoInterno(new PostLocalizacaoExecucaoDTO
            {
                RotaExecucaoId = execucao.Id,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                DataHora = dto.DataHora,
                GpsSimulado = dto.GpsSimulado,
                PrecisaoEmMetros = dto.PrecisaoEmMetros,
                VelocidadeMetrosPorSegundo = dto.VelocidadeMetrosPorSegundo,
                DirecaoGraus = dto.DirecaoGraus,
                AltitudeMetros = dto.AltitudeMetros,
                FonteCaptura = dto.FonteCaptura
            }, (int)execucao.MotoristaId, validarStatusAtivo: false);
        }

        public int ObterMotoristaIdPorServidor(int servidorId)
        {
            var sql = "SELECT Id FROM motorista WHERE ServidorId = @ServidorId LIMIT 1";
            return _repositorioMotorista.ConsultaDapper<int>(sql, new { @ServidorId = servidorId }).FirstOrDefault();
        }

        private void SalvarPontoInterno(PostLocalizacaoExecucaoDTO dto, int motoristaId, bool validarStatusAtivo)
        {
            var execucao = ObterExecucaoValida(dto.RotaExecucaoId, motoristaId, validarStatusAtivo);

            var sql = @"INSERT INTO rotaexecucaolocalizacao
                            (OrganizacaoId, RotaExecucaoId, Latitude, Longitude, PrecisaoEmMetros, VelocidadeMetrosPorSegundo, DirecaoGraus, AltitudeMetros, GpsSimulado, FonteCaptura, DataHoraCaptura)
                        VALUES
                            (@OrgId, @ExecId, @Lat, @Lon, @Precisao, @Velocidade, @Direcao, @Altitude, @Mock, @Fonte, @Data);
                        SELECT LAST_INSERT_ID();";
            var localizacaoId = _repositorioLocalizacao.ConsultaDapper<long>(sql, new
            {
                @OrgId = execucao.OrganizacaoId,
                @ExecId = dto.RotaExecucaoId,
                @Lat = dto.Latitude,
                @Lon = dto.Longitude,
                @Precisao = dto.PrecisaoEmMetros,
                @Velocidade = dto.VelocidadeMetrosPorSegundo,
                @Direcao = dto.DirecaoGraus,
                @Altitude = dto.AltitudeMetros,
                @Mock = dto.GpsSimulado,
                @Fonte = dto.FonteCaptura ?? 0,
                @Data = dto.DataHora
            }).First();

            var sqlUpdate = @"UPDATE rotaexecucao
                              SET UltimaLatitude = @Lat,
                                  UltimaLongitude = @Lon,
                                  UltimaAtualizacaoEm = @Data,
                                  GpsSimuladoUltimaLeitura = @Mock,
                                  DataHoraUltimaComunicacaoApp = @Agora,
                                  DataAlteracao = @Agora
                              WHERE Id = @Id";
            _repositorioExecucao.ExecutarComando(sqlUpdate, new
            {
                @Lat = dto.Latitude,
                @Lon = dto.Longitude,
                @Data = dto.DataHora,
                @Mock = dto.GpsSimulado,
                @Agora = DateTime.Now,
                @Id = dto.RotaExecucaoId
            });

            RegistrarDesvioSeNecessario(execucao.RotaId, execucao.OrganizacaoId, dto, localizacaoId);
        }

        private void RegistrarDesvioSeNecessario(int rotaId, int organizacaoId, PostLocalizacaoExecucaoDTO dto, long localizacaoId)
        {
            var sqlRotaInfo = "SELECT PolylineOficial FROM rota WHERE Id = @RotaId";
            var polyline = _repositorioRota.ConsultaDapper<string>(sqlRotaInfo, new { @RotaId = rotaId }).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(polyline))
                return;

            if (!double.TryParse(dto.Latitude, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat))
                return;

            if (!double.TryParse(dto.Longitude, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                return;

            var driverCoord = new Utilitarios.PolylineUtils.Coordinate { Latitude = lat, Longitude = lon };
            var polylinePath = Utilitarios.PolylineUtils.Decode(polyline);

            double minDistance = double.MaxValue;
            for (int i = 0; i < polylinePath.Count - 1; i++)
            {
                var dist = Utilitarios.PolylineUtils.DistanceToSegment(driverCoord, polylinePath[i], polylinePath[i + 1]);
                if (dist < minDistance) minDistance = dist;
            }

            if (minDistance <= 500)
                return;

            var sqlDesvio = @"INSERT INTO rotaexecucaodesvio
                                (OrganizacaoId, RotaExecucaoId, RotaExecucaoLocalizacaoId, Latitude, Longitude, DistanciaEmMetros, DataHoraDeteccao)
                              VALUES
                                (@OrgId, @ExecId, @LocalizacaoId, @Lat, @Lon, @Dist, @Data)";
            _repositorioDesvio.ExecutarComando(sqlDesvio, new
            {
                @OrgId = organizacaoId,
                @ExecId = dto.RotaExecucaoId,
                @LocalizacaoId = localizacaoId,
                @Lat = dto.Latitude,
                @Lon = dto.Longitude,
                @Dist = minDistance,
                @Data = dto.DataHora
            });
        }

        private void SalvarEventoExecucao(int rotaExecucaoId, int tipoEvento, int? paradaRotaId, int? unidadeId, bool? entregue, string? observacao, string? latitude, string? longitude, bool gpsSimulado, DateTime dataHoraEvento)
        {
            var sqlOrg = "SELECT OrganizacaoId FROM rotaexecucao WHERE Id = @Id";
            var organizacaoId = _repositorioExecucao.ConsultaDapper<int>(sqlOrg, new { @Id = rotaExecucaoId }).First();

            var sql = @"INSERT INTO rotaexecucaoevento
                            (OrganizacaoId, RotaExecucaoId, ParadaRotaId, UnidadeId, Sequencia, TipoEvento, StatusEvento, Entregue, Observacao, Latitude, Longitude, GpsSimulado, DataHoraEvento, UsuarioIdRegistro)
                        VALUES
                            (@OrgId, @ExecId, @ParadaRotaId, @UnidadeId, @Sequencia, @TipoEvento, @StatusEvento, @Entregue, @Observacao, @Latitude, @Longitude, @GpsSimulado, @DataHoraEvento, NULL)";

            var sequencia = _repositorioEvento.ConsultaDapper<int>("SELECT COALESCE(MAX(Sequencia), 0) + 1 FROM rotaexecucaoevento WHERE RotaExecucaoId = @ExecId", new { @ExecId = rotaExecucaoId }).FirstOrDefault();
            _repositorioEvento.ExecutarComando(sql, new
            {
                @OrgId = organizacaoId,
                @ExecId = rotaExecucaoId,
                @ParadaRotaId = paradaRotaId,
                @UnidadeId = unidadeId,
                @Sequencia = sequencia,
                @TipoEvento = tipoEvento,
                @StatusEvento = entregue.HasValue ? 1 : (int?)null,
                @Entregue = entregue,
                @Observacao = observacao,
                @Latitude = latitude,
                @Longitude = longitude,
                @GpsSimulado = gpsSimulado,
                @DataHoraEvento = dataHoraEvento
            });

            _repositorioExecucao.ExecutarComando(
                "UPDATE rotaexecucao SET DataHoraUltimaComunicacaoApp = @Agora, DataAlteracao = @Agora WHERE Id = @Id",
                new
                {
                    Id = rotaExecucaoId,
                    Agora = DateTime.Now
                });
        }

        private void ValidarChecklistParaInicio(IniciarRotaAppDTO dto, int motoristaId)
        {
            var sqlChecklist = @"
                SELECT COUNT(1)
                FROM checklistexecucao
                WHERE Id = @ChecklistId
                  AND MotoristaId = @MotoristaId
                  AND RotaId = @RotaId
                  AND VeiculoId = @VeiculoId";

            var valido = _repositorioChecklistExec.ConsultaDapper<int>(sqlChecklist, new
            {
                @ChecklistId = dto.ChecklistExecucaoId,
                @MotoristaId = motoristaId,
                @RotaId = dto.RotaId,
                @VeiculoId = dto.VeiculoId
            }).FirstOrDefault() > 0;

            if (!valido)
                throw new ApplicationException("Checklist invalido para o motorista, rota e veiculo selecionados.");
        }

        private ExecucaoValidacaoDTO ObterExecucaoValida(int rotaExecucaoId, int motoristaId, bool validarAtiva = false)
        {
            var sql = @"
                SELECT e.Id, e.OrganizacaoId, e.RotaId, e.Status, e.MotoristaId, e.UltimaLatitude, e.UltimaLongitude, e.GpsSimuladoUltimaLeitura,
                       r.UnidadeOrigemId, r.UnidadeDestinoId
                FROM rotaexecucao e
                INNER JOIN rota r ON r.Id = e.RotaId
                WHERE e.Id = @ExecId";

            var execucao = _repositorioExecucao.ConsultaDapper<ExecucaoValidacaoDTO>(sql, new { @ExecId = rotaExecucaoId }).FirstOrDefault();
            if (execucao == null)
                throw new ApplicationException("Execucao da rota nao encontrada.");

            if ((int)execucao.MotoristaId != motoristaId)
                throw new ApplicationException("A execucao informada nao pertence ao motorista autenticado.");

            if (validarAtiva && (int)execucao.Status != StatusExecucaoEmAndamento && (int)execucao.Status != StatusExecucaoPausada)
                throw new ApplicationException("A execucao informada nao esta ativa.");

            return execucao;
        }

        private int? CriarChecklistOfflineSeNecessario(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, SincronizarRotaOfflineDTO dto, int motoristaId, int organizacaoId, DateTime agora)
        {
            if (dto.ChecklistExecucaoId.HasValue && dto.ChecklistExecucaoId.Value > 0)
                return dto.ChecklistExecucaoId.Value;

            if (dto.ItensChecklist == null || !dto.ItensChecklist.Any())
                return null;

            var checklistExecucaoId = conn.QuerySingle<int>(
                @"INSERT INTO checklistexecucao (OrganizacaoId, VeiculoId, MotoristaId, RotaId, DataHora)
                  VALUES (@OrganizacaoId, @VeiculoId, @MotoristaId, @RotaId, @DataHora);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    OrganizacaoId = organizacaoId,
                    dto.VeiculoId,
                    MotoristaId = motoristaId,
                    dto.RotaId,
                    DataHora = dto.DataHoraInicioLocal
                },
                transaction);

            foreach (var itemId in dto.ItensChecklist.Distinct())
            {
                conn.Execute(
                    "INSERT INTO checklistexecucaoitem (ChecklistExecucaoId, ChecklistItemId, Marcado) VALUES (@ChecklistExecucaoId, @ItemId, 1)",
                    new { ChecklistExecucaoId = checklistExecucaoId, ItemId = itemId },
                    transaction);
            }

            return checklistExecucaoId;
        }

        private void SalvarEventosOffline(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, SincronizarRotaOfflineDTO dto, int execucaoId, RotaSincronizacaoOfflineLookupDTO rota, DateTime agora)
        {
            var eventos = dto.Eventos
                .Where(e => !string.IsNullOrWhiteSpace(e.ClientEventId))
                .OrderBy(e => e.DataHoraEvento)
                .ToList();

            foreach (var evento in eventos)
            {
                if (SincronizacaoJaRegistrada(conn, transaction, dto.LocalExecucaoId, evento.ClientEventId, "evento"))
                    continue;

                var unidadeId = evento.UnidadeId;
                if (!unidadeId.HasValue && evento.TipoEvento == TipoEventoOrigem)
                    unidadeId = rota.UnidadeOrigemId;
                if (!unidadeId.HasValue && evento.TipoEvento == TipoEventoDestino)
                    unidadeId = rota.UnidadeDestinoId;

                var sequencia = conn.QueryFirst<int>(
                    "SELECT COALESCE(MAX(Sequencia), 0) + 1 FROM rotaexecucaoevento WHERE RotaExecucaoId = @ExecucaoId",
                    new { ExecucaoId = execucaoId },
                    transaction);

                conn.Execute(
                    @"INSERT INTO rotaexecucaoevento
                        (OrganizacaoId, RotaExecucaoId, ParadaRotaId, UnidadeId, Sequencia, TipoEvento, StatusEvento,
                         Entregue, Observacao, Latitude, Longitude, GpsSimulado, DataHoraEvento, RegistradoOffline,
                         DataHoraRegistroLocal, DataHoraSincronizacao, IdentificadorDispositivo, LocalExecucaoId,
                         ClientEventId, UsuarioIdRegistro, DataCriacao)
                      VALUES
                        (@OrganizacaoId, @ExecucaoId, @ParadaRotaId, @UnidadeId, @Sequencia, @TipoEvento, @StatusEvento,
                         @Entregue, @Observacao, @Latitude, @Longitude, @GpsSimulado, @DataHoraEvento, 1,
                         @DataHoraRegistroLocal, @DataHoraSincronizacao, @IdentificadorDispositivo, @LocalExecucaoId,
                         @ClientEventId, NULL, @Agora)",
                    new
                    {
                        rota.OrganizacaoId,
                        ExecucaoId = execucaoId,
                        evento.ParadaRotaId,
                        UnidadeId = unidadeId,
                        Sequencia = sequencia,
                        evento.TipoEvento,
                        StatusEvento = evento.Entregue.HasValue ? 1 : (int?)null,
                        evento.Entregue,
                        evento.Observacao,
                        evento.Latitude,
                        evento.Longitude,
                        evento.GpsSimulado,
                        evento.DataHoraEvento,
                        DataHoraRegistroLocal = evento.DataHoraRegistroLocal == default ? evento.DataHoraEvento : evento.DataHoraRegistroLocal,
                        DataHoraSincronizacao = agora,
                        IdentificadorDispositivo = evento.IdentificadorDispositivo ?? dto.IdentificadorDispositivo,
                        dto.LocalExecucaoId,
                        evento.ClientEventId,
                        Agora = agora
                    },
                    transaction);

                RegistrarSincronizacaoOffline(conn, transaction, rota.OrganizacaoId, execucaoId, dto.LocalExecucaoId, evento.ClientEventId, "evento", evento.IdentificadorDispositivo ?? dto.IdentificadorDispositivo, evento.DataHoraRegistroLocal == default ? evento.DataHoraEvento : evento.DataHoraRegistroLocal, agora);
            }

            if (dto.DataHoraFimLocal.HasValue && !SincronizacaoJaRegistrada(conn, transaction, dto.LocalExecucaoId, $"{dto.LocalExecucaoId}_fim", "fim_rota"))
            {
                var sequencia = conn.QueryFirst<int>(
                    "SELECT COALESCE(MAX(Sequencia), 0) + 1 FROM rotaexecucaoevento WHERE RotaExecucaoId = @ExecucaoId",
                    new { ExecucaoId = execucaoId },
                    transaction);

                conn.Execute(
                    @"INSERT INTO rotaexecucaoevento
                        (OrganizacaoId, RotaExecucaoId, Sequencia, TipoEvento, StatusEvento, Entregue, Observacao,
                         Latitude, Longitude, GpsSimulado, DataHoraEvento, RegistradoOffline, DataHoraRegistroLocal,
                         DataHoraSincronizacao, IdentificadorDispositivo, LocalExecucaoId, ClientEventId, UsuarioIdRegistro, DataCriacao)
                      VALUES
                        (@OrganizacaoId, @ExecucaoId, @Sequencia, @TipoEvento, 1, 1, @Observacao, @Latitude, @Longitude,
                         @GpsSimulado, @DataHoraEvento, 1, @DataHoraRegistroLocal, @DataHoraSincronizacao,
                         @IdentificadorDispositivo, @LocalExecucaoId, @ClientEventId, NULL, @Agora)",
                    new
                    {
                        rota.OrganizacaoId,
                        ExecucaoId = execucaoId,
                        Sequencia = sequencia,
                        TipoEvento = TipoEventoFimRota,
                        Observacao = dto.ObservacaoFim,
                        Latitude = ObterUltimaLatitude(dto),
                        Longitude = ObterUltimaLongitude(dto),
                        GpsSimulado = ObterUltimoGpsSimulado(dto),
                        DataHoraEvento = dto.DataHoraFimLocal.Value,
                        DataHoraRegistroLocal = dto.DataHoraFimLocal.Value,
                        DataHoraSincronizacao = agora,
                        dto.IdentificadorDispositivo,
                        dto.LocalExecucaoId,
                        ClientEventId = $"{dto.LocalExecucaoId}_fim",
                        Agora = agora
                    },
                    transaction);

                RegistrarSincronizacaoOffline(conn, transaction, rota.OrganizacaoId, execucaoId, dto.LocalExecucaoId, $"{dto.LocalExecucaoId}_fim", "fim_rota", dto.IdentificadorDispositivo, dto.DataHoraFimLocal.Value, agora);
            }
        }

        private void SalvarLocalizacoesOffline(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, SincronizarRotaOfflineDTO dto, int execucaoId, int organizacaoId, int motoristaId, DateTime agora)
        {
            foreach (var localizacao in dto.Localizacoes.Where(l => !string.IsNullOrWhiteSpace(l.ClientEventId)).OrderBy(l => l.DataHora))
            {
                if (SincronizacaoJaRegistrada(conn, transaction, dto.LocalExecucaoId, localizacao.ClientEventId, "localizacao"))
                    continue;

                conn.Execute(
                    @"INSERT INTO rotaexecucaolocalizacao
                        (OrganizacaoId, RotaExecucaoId, Latitude, Longitude, PrecisaoEmMetros, VelocidadeMetrosPorSegundo,
                         DirecaoGraus, AltitudeMetros, GpsSimulado, FonteCaptura, DataHoraCaptura, RegistradoOffline,
                         DataHoraRegistroLocal, DataHoraSincronizacao, IdentificadorDispositivo, LocalExecucaoId,
                         ClientEventId, DataCriacao)
                      VALUES
                        (@OrganizacaoId, @ExecucaoId, @Latitude, @Longitude, @PrecisaoEmMetros, @VelocidadeMetrosPorSegundo,
                         @DirecaoGraus, @AltitudeMetros, @GpsSimulado, @FonteCaptura, @DataHoraCaptura, 1,
                         @DataHoraRegistroLocal, @DataHoraSincronizacao, @IdentificadorDispositivo, @LocalExecucaoId,
                         @ClientEventId, @Agora)",
                    new
                    {
                        OrganizacaoId = organizacaoId,
                        ExecucaoId = execucaoId,
                        localizacao.Latitude,
                        localizacao.Longitude,
                        localizacao.PrecisaoEmMetros,
                        localizacao.VelocidadeMetrosPorSegundo,
                        localizacao.DirecaoGraus,
                        localizacao.AltitudeMetros,
                        localizacao.GpsSimulado,
                        FonteCaptura = localizacao.FonteCaptura ?? 0,
                        DataHoraCaptura = localizacao.DataHora,
                        DataHoraRegistroLocal = localizacao.DataHoraRegistroLocal == default ? localizacao.DataHora : localizacao.DataHoraRegistroLocal,
                        DataHoraSincronizacao = agora,
                        IdentificadorDispositivo = localizacao.IdentificadorDispositivo ?? dto.IdentificadorDispositivo,
                        dto.LocalExecucaoId,
                        localizacao.ClientEventId,
                        Agora = agora
                    },
                    transaction);

                conn.Execute(
                    @"UPDATE rotaexecucao
                      SET UltimaLatitude = @Latitude,
                          UltimaLongitude = @Longitude,
                          UltimaAtualizacaoEm = @DataHora,
                          GpsSimuladoUltimaLeitura = @GpsSimulado,
                          DataHoraUltimaComunicacaoApp = @Agora,
                          DataAlteracao = @Agora
                      WHERE Id = @ExecucaoId",
                    new
                    {
                        localizacao.Latitude,
                        localizacao.Longitude,
                        localizacao.DataHora,
                        localizacao.GpsSimulado,
                        Agora = agora,
                        ExecucaoId = execucaoId
                    },
                    transaction);

                RegistrarSincronizacaoOffline(conn, transaction, organizacaoId, execucaoId, dto.LocalExecucaoId, localizacao.ClientEventId, "localizacao", localizacao.IdentificadorDispositivo ?? dto.IdentificadorDispositivo, localizacao.DataHoraRegistroLocal == default ? localizacao.DataHora : localizacao.DataHoraRegistroLocal, agora);
            }
        }

        private void SalvarPausasOffline(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, SincronizarRotaOfflineDTO dto, int execucaoId, int organizacaoId, DateTime agora)
        {
            foreach (var pausa in dto.Pausas.Where(p => !string.IsNullOrWhiteSpace(p.ClientEventId)).OrderBy(p => p.DataHoraInicio))
            {
                if (SincronizacaoJaRegistrada(conn, transaction, dto.LocalExecucaoId, pausa.ClientEventId, "pausa"))
                {
                    if (pausa.DataHoraFim.HasValue && !SincronizacaoJaRegistrada(conn, transaction, dto.LocalExecucaoId, $"{pausa.ClientEventId}_fim", "pausa_fim"))
                    {
                        conn.Execute(
                            @"UPDATE rotaexecucaopausa
                              SET DataHoraFim = @DataHoraFim,
                                  LatitudeFim = @LatitudeFim,
                                  LongitudeFim = @LongitudeFim,
                                  GpsSimuladoFim = @GpsSimuladoFim,
                                  DataHoraSincronizacao = @DataHoraSincronizacao
                              WHERE RotaExecucaoId = @ExecucaoId
                                AND ClientEventId = @ClientEventId",
                            new
                            {
                                pausa.DataHoraFim,
                                pausa.LatitudeFim,
                                pausa.LongitudeFim,
                                pausa.GpsSimuladoFim,
                                DataHoraSincronizacao = agora,
                                ExecucaoId = execucaoId,
                                pausa.ClientEventId
                            },
                            transaction);

                        RegistrarSincronizacaoOffline(conn, transaction, organizacaoId, execucaoId, dto.LocalExecucaoId, $"{pausa.ClientEventId}_fim", "pausa_fim", pausa.IdentificadorDispositivo ?? dto.IdentificadorDispositivo, pausa.DataHoraFim.Value, agora);
                    }
                    continue;
                }

                conn.Execute(
                    @"INSERT INTO rotaexecucaopausa
                        (OrganizacaoId, RotaExecucaoId, Motivo, DataHoraInicio, LatitudeInicio, LongitudeInicio,
                         GpsSimuladoInicio, DataHoraFim, LatitudeFim, LongitudeFim, GpsSimuladoFim, RegistradoOffline,
                         DataHoraRegistroLocal, DataHoraSincronizacao, IdentificadorDispositivo, LocalExecucaoId,
                         ClientEventId, UsuarioIdRegistro, DataCriacao)
                      VALUES
                        (@OrganizacaoId, @ExecucaoId, @Motivo, @DataHoraInicio, @LatitudeInicio, @LongitudeInicio,
                         @GpsSimuladoInicio, @DataHoraFim, @LatitudeFim, @LongitudeFim, @GpsSimuladoFim, 1,
                         @DataHoraRegistroLocal, @DataHoraSincronizacao, @IdentificadorDispositivo, @LocalExecucaoId,
                         @ClientEventId, NULL, @Agora)",
                    new
                    {
                        OrganizacaoId = organizacaoId,
                        ExecucaoId = execucaoId,
                        Motivo = string.IsNullOrWhiteSpace(pausa.Motivo) ? "Pausa informada pelo app offline" : pausa.Motivo,
                        pausa.DataHoraInicio,
                        pausa.LatitudeInicio,
                        pausa.LongitudeInicio,
                        pausa.GpsSimuladoInicio,
                        pausa.DataHoraFim,
                        pausa.LatitudeFim,
                        pausa.LongitudeFim,
                        pausa.GpsSimuladoFim,
                        DataHoraRegistroLocal = pausa.DataHoraRegistroLocal == default ? pausa.DataHoraInicio : pausa.DataHoraRegistroLocal,
                        DataHoraSincronizacao = agora,
                        IdentificadorDispositivo = pausa.IdentificadorDispositivo ?? dto.IdentificadorDispositivo,
                        dto.LocalExecucaoId,
                        pausa.ClientEventId,
                        Agora = agora
                    },
                    transaction);

                RegistrarSincronizacaoOffline(conn, transaction, organizacaoId, execucaoId, dto.LocalExecucaoId, pausa.ClientEventId, "pausa", pausa.IdentificadorDispositivo ?? dto.IdentificadorDispositivo, pausa.DataHoraRegistroLocal == default ? pausa.DataHoraInicio : pausa.DataHoraRegistroLocal, agora);
                if (pausa.DataHoraFim.HasValue)
                    RegistrarSincronizacaoOffline(conn, transaction, organizacaoId, execucaoId, dto.LocalExecucaoId, $"{pausa.ClientEventId}_fim", "pausa_fim", pausa.IdentificadorDispositivo ?? dto.IdentificadorDispositivo, pausa.DataHoraFim.Value, agora);
            }
        }

        private void AtualizarResumoOfflineExecucao(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, SincronizarRotaOfflineDTO dto, int execucaoId, DateTime agora)
        {
            conn.Execute(
                @"UPDATE rotaexecucao
                  SET Status = CASE WHEN @DataHoraFimLocal IS NOT NULL THEN @StatusFinalizada ELSE Status END,
                      DataHoraFim = COALESCE(@DataHoraFimLocal, DataHoraFim),
                      ObservacaoFim = COALESCE(@ObservacaoFim, ObservacaoFim),
                      PossuiRegistroOffline = 1,
                      ExecucaoOfflineCompleta = CASE WHEN @DataHoraFimLocal IS NOT NULL THEN 1 ELSE ExecucaoOfflineCompleta END,
                      DataHoraPrimeiroRegistroOffline = CASE
                          WHEN DataHoraPrimeiroRegistroOffline IS NULL THEN @PrimeiroOffline
                          WHEN @PrimeiroOffline IS NULL THEN DataHoraPrimeiroRegistroOffline
                          WHEN @PrimeiroOffline < DataHoraPrimeiroRegistroOffline THEN @PrimeiroOffline
                          ELSE DataHoraPrimeiroRegistroOffline END,
                      DataHoraUltimoRegistroOffline = CASE
                          WHEN DataHoraUltimoRegistroOffline IS NULL THEN @UltimoOffline
                          WHEN @UltimoOffline IS NULL THEN DataHoraUltimoRegistroOffline
                          WHEN @UltimoOffline > DataHoraUltimoRegistroOffline THEN @UltimoOffline
                          ELSE DataHoraUltimoRegistroOffline END,
                      DataHoraUltimaComunicacaoApp = @Agora,
                      DataAlteracao = @Agora
                  WHERE Id = @ExecucaoId",
                new
                {
                    dto.DataHoraFimLocal,
                    StatusFinalizada = StatusExecucaoFinalizada,
                    dto.ObservacaoFim,
                    PrimeiroOffline = ObterPrimeiroRegistroOffline(dto),
                    UltimoOffline = ObterUltimoRegistroOffline(dto),
                    Agora = agora,
                    ExecucaoId = execucaoId
                },
                transaction);
        }

        private bool SincronizacaoJaRegistrada(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, string localExecucaoId, string clientEventId, string tipoRegistro)
        {
            return conn.QueryFirst<int>(
                @"SELECT COUNT(1)
                  FROM rotaexecucaosincronizacaooffline
                  WHERE LocalExecucaoId = @LocalExecucaoId
                    AND ClientEventId = @ClientEventId
                    AND TipoRegistro = @TipoRegistro",
                new { LocalExecucaoId = localExecucaoId, ClientEventId = clientEventId, TipoRegistro = tipoRegistro },
                transaction) > 0;
        }

        private void RegistrarSincronizacaoOffline(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, int organizacaoId, int execucaoId, string localExecucaoId, string clientEventId, string tipoRegistro, string? identificadorDispositivo, DateTime? dataHoraRegistroLocal, DateTime dataHoraSincronizacao)
        {
            if (SincronizacaoJaRegistrada(conn, transaction, localExecucaoId, clientEventId, tipoRegistro))
                return;

            conn.Execute(
                @"INSERT INTO rotaexecucaosincronizacaooffline
                    (OrganizacaoId, RotaExecucaoId, LocalExecucaoId, ClientEventId, TipoRegistro,
                     IdentificadorDispositivo, DataHoraRegistroLocal, DataHoraSincronizacao, DataCriacao)
                  VALUES
                    (@OrganizacaoId, @ExecucaoId, @LocalExecucaoId, @ClientEventId, @TipoRegistro,
                     @IdentificadorDispositivo, @DataHoraRegistroLocal, @DataHoraSincronizacao, @DataHoraSincronizacao)",
                new
                {
                    OrganizacaoId = organizacaoId,
                    ExecucaoId = execucaoId,
                    LocalExecucaoId = localExecucaoId,
                    ClientEventId = clientEventId,
                    TipoRegistro = tipoRegistro,
                    IdentificadorDispositivo = identificadorDispositivo,
                    DataHoraRegistroLocal = dataHoraRegistroLocal,
                    DataHoraSincronizacao = dataHoraSincronizacao
                },
                transaction);
        }

        private DateTime? ObterPrimeiroRegistroOffline(SincronizarRotaOfflineDTO dto)
        {
            var datas = new List<DateTime> { dto.DataHoraInicioLocal };
            datas.AddRange(dto.Eventos.Select(e => e.DataHoraEvento));
            datas.AddRange(dto.Localizacoes.Select(l => l.DataHora));
            datas.AddRange(dto.Pausas.Select(p => p.DataHoraInicio));
            if (dto.DataHoraFimLocal.HasValue) datas.Add(dto.DataHoraFimLocal.Value);
            return datas.Any() ? datas.Min() : null;
        }

        private DateTime? ObterUltimoRegistroOffline(SincronizarRotaOfflineDTO dto)
        {
            var datas = new List<DateTime> { dto.DataHoraInicioLocal };
            datas.AddRange(dto.Eventos.Select(e => e.DataHoraEvento));
            datas.AddRange(dto.Localizacoes.Select(l => l.DataHora));
            datas.AddRange(dto.Pausas.Select(p => p.DataHoraFim ?? p.DataHoraInicio));
            if (dto.DataHoraFimLocal.HasValue) datas.Add(dto.DataHoraFimLocal.Value);
            return datas.Any() ? datas.Max() : null;
        }

        private DateTime? ObterUltimaDataHora(SincronizarRotaOfflineDTO dto)
        {
            return dto.Localizacoes.Any()
                ? dto.Localizacoes.Max(l => l.DataHora)
                : dto.DataHoraFimLocal ?? dto.DataHoraInicioLocal;
        }

        private string? ObterUltimaLatitude(SincronizarRotaOfflineDTO dto)
        {
            return dto.Localizacoes.OrderBy(l => l.DataHora).LastOrDefault()?.Latitude ?? dto.LatitudeInicio;
        }

        private string? ObterUltimaLongitude(SincronizarRotaOfflineDTO dto)
        {
            return dto.Localizacoes.OrderBy(l => l.DataHora).LastOrDefault()?.Longitude ?? dto.LongitudeInicio;
        }

        private bool ObterUltimoGpsSimulado(SincronizarRotaOfflineDTO dto)
        {
            return dto.Localizacoes.OrderBy(l => l.DataHora).LastOrDefault()?.GpsSimulado ?? dto.GpsSimuladoInicio;
        }

        private string ObterSomenteNumeros(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return new string(texto.Where(char.IsDigit).ToArray());
        }

        private List<ParadaRotaDTO> ObterParadasOffline(int rotaId)
        {
            var campoObservacaoCadastro = ColunaExiste("paradarota", "ObservacaoCadastro")
                ? "ObservacaoCadastro"
                : "NULL as ObservacaoCadastro";

            var sql = @"
                SELECT Id, Endereco, Latitude, Longitude, Link, " + campoObservacaoCadastro + @"
                FROM paradarota
                WHERE RotaId = @RotaId
                ORDER BY Ordem ASC, Id ASC";

            return _repositorioParada.ConsultaDapper<ParadaRotaDTO>(sql, new { @RotaId = rotaId }).ToList();
        }

        private bool ColunaExiste(string tabela, string coluna)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @Tabela
                  AND COLUMN_NAME = @Coluna";

            return _repositorioParada
                .ConsultaDapper<int>(sql, new { @Tabela = tabela, @Coluna = coluna })
                .FirstOrDefault() > 0;
        }

        private bool RotaPodeExecutarNoPeriodo(RotaOfflineConsultaDTO rota, DateTime inicio, DateTime fim)
        {
            if (!rota.Recorrente)
                return rota.DataParaExecucao != null &&
                       rota.DataParaExecucao.Value.Date >= inicio &&
                       rota.DataParaExecucao.Value.Date <= fim;

            if (rota.DataInicio != null && rota.DataInicio.Value.Date > fim) return false;
            if (rota.DataFim != null && rota.DataFim.Value.Date < inicio) return false;

            if (!rota.DiasSemana.HasValue) return false;

            var data = inicio;
            var dias = (eFlagDiaSemana)rota.DiasSemana.Value;
            while (data <= fim)
            {
                if (dias.HasFlag(ObterFlagHoje(data))) return true;
                data = data.AddDays(1);
            }

            return false;
        }

        private eFlagDiaSemana ObterFlagHoje(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Sunday => eFlagDiaSemana.Domingo,
                DayOfWeek.Monday => eFlagDiaSemana.Segunda,
                DayOfWeek.Tuesday => eFlagDiaSemana.Terca,
                DayOfWeek.Wednesday => eFlagDiaSemana.Quarta,
                DayOfWeek.Thursday => eFlagDiaSemana.Quinta,
                DayOfWeek.Friday => eFlagDiaSemana.Sexta,
                DayOfWeek.Saturday => eFlagDiaSemana.Sabado,
                _ => eFlagDiaSemana.Nenhum
            };
        }

        private class RotaOfflineConsultaDTO
        {
            public int Id { get; set; }
            public string Codigo { get; set; }
            public string Nome { get; set; }
            public string Descricao { get; set; }
            public bool Recorrente { get; set; }
            public DateTime? DataParaExecucao { get; set; }
            public DateTime? DataInicio { get; set; }
            public DateTime? DataFim { get; set; }
            public int? DiasSemana { get; set; }
            public bool PermitePausa { get; set; }
            public int QuantidadePausas { get; set; }
            public int? UnidadeOrigemId { get; set; }
            public int? UnidadeDestinoId { get; set; }
            public string? NomeUnidadeOrigem { get; set; }
            public string? NomeUnidadeDestino { get; set; }
            public string? OrigemLatitudeRota { get; set; }
            public string? OrigemLongitudeRota { get; set; }
            public string? DestinoLatitudeRota { get; set; }
            public string? DestinoLongitudeRota { get; set; }
        }

        private class RotaSincronizacaoOfflineLookupDTO
        {
            public int Id { get; set; }
            public int OrganizacaoId { get; set; }
            public int? UnidadeOrigemId { get; set; }
            public int? UnidadeDestinoId { get; set; }
        }
    }
}
