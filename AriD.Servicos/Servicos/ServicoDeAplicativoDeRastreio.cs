using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

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
                                (OrganizacaoId, RotaId, MotoristaId, VeiculoId, ChecklistExecucaoId, Status, DataHoraInicio, UsuarioIdInicio, ObservacaoInicio, UltimaLatitude, UltimaLongitude, UltimaAtualizacaoEm, GpsSimuladoUltimaLeitura)
                              VALUES
                                ((SELECT OrganizacaoId FROM rota WHERE Id = @RotaId), @RotaId, @MotoristaId, @VeiculoId, @ChecklistId, @Status, @Agora, NULL, @ObsInicio, @Lat, @Lon, NULL, @GpsSimulado);
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
                       uo.Nome as NomeUnidadeOrigem, ud.Nome as NomeUnidadeDestino
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

            var sqlParadas = @"
                SELECT
                    p.Id,
                    p.Endereco,
                    p.Latitude,
                    p.Longitude,
                    p.Link,
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
                OrigemEntregue = origem?.Entregue == null ? null : Convert.ToBoolean(origem.Entregue),
                OrigemObservacao = origem?.Observacao,
                OrigemConcluidaEm = origem?.DataHoraEvento != null ? ((DateTime)origem.DataHoraEvento).ToString("yyyy-MM-ddTHH:mm:ss") : null,
                OrigemLatitude = origem?.Latitude,
                OrigemLongitude = origem?.Longitude,
                NomeUnidadeDestino = execucao.NomeUnidadeDestino,
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

            _repositorioExecucao.ExecutarComando("UPDATE rotaexecucao SET Status = @Status, DataAlteracao = @Agora WHERE Id = @Id", new
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

            _repositorioExecucao.ExecutarComando("UPDATE rotaexecucao SET Status = @Status, DataAlteracao = @Agora WHERE Id = @Id", new
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

        private string ObterSomenteNumeros(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return new string(texto.Where(char.IsDigit).ToArray());
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
    }
}
