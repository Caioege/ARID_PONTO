using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using MySqlConnector;
using System.Data;
using Newtonsoft.Json;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeAplicativoDeRastreio : IServicoDeAplicativoDeRastreio
    {
        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<Motorista> _repositorioMotorista;
        private readonly IRepositorio<Rota> _repositorioRota;
        private readonly IRepositorio<RotaExecucao> _repositorioExecucao;
        private readonly IRepositorio<LocalizacaoRota> _repositorioLocalizacao;
        private readonly IRepositorio<ParadaRota> _repositorioParada;
        private readonly IRepositorio<ChecklistExecucao> _repositorioChecklistExec;
        private readonly IRepositorio<RotaOcorrenciaDesvio> _repositorioDesvio;

        public ServicoDeAplicativoDeRastreio(
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<Motorista> repositorioMotorista,
            IRepositorio<Rota> repositorioRota,
            IRepositorio<RotaExecucao> repositorioExecucao,
            IRepositorio<LocalizacaoRota> repositorioLocalizacao,
            IRepositorio<ParadaRota> repositorioParada,
            IRepositorio<ChecklistExecucao> repositorioChecklistExec,
            IRepositorio<RotaOcorrenciaDesvio> repositorioDesvio)
        {
            _repositorioServidor = repositorioServidor;
            _repositorioMotorista = repositorioMotorista;
            _repositorioRota = repositorioRota;
            _repositorioExecucao = repositorioExecucao;
            _repositorioLocalizacao = repositorioLocalizacao;
            _repositorioParada = repositorioParada;
            _repositorioChecklistExec = repositorioChecklistExec;
            _repositorioDesvio = repositorioDesvio;
        }

        public AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais)
        {
            try
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
                    // Acompanhante: Deve estar vinculado a pelo menos uma rota
                    queryAcesso += @" and exists (select 1 from rotaprofissional rp where rp.ServidorId = s.Id) ";
                }
                else
                {
                    // Motorista: Deve ter cadastro ativo na tabela Motorista e senha válida
                    queryAcesso += @" and (IF(s.SenhaPersonalizadaDeAcesso IS NULL, DATE_FORMAT(p.DataDeNascimento, '%d%m%Y') = @SENHA, s.SenhaPersonalizadaDeAcesso = @SENHACRIPTOGRAFADA) = true) 
                                      and m.Id IS NOT NULL ";
                }

                queryAcesso += @" limit 1";

                var result = _repositorioServidor.ConsultaDapper<AutenticacaoAppDTO>(queryAcesso, new
                {
                    @USUARIO = ObterSomenteNumeros(credenciais.Usuario),
                    @SENHA = ObterSomenteNumeros(credenciais.Senha),
                    @SENHACRIPTOGRAFADA = Criptografia.CriptografarSenha(credenciais.Senha)
                }).FirstOrDefault();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
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
                WHERE r.Situacao = 1 AND (r.MotoristaId = @ID OR r.MotoristaSecundarioId = @ID)"; // 1 = Ativa

            var todas = _repositorioRota.ConsultaDapper<dynamic>(sql, new { @ID = motoristaId }).ToList();
            var hoje = DateTime.Now.Date;
            var flagHoje = ObterFlagHoje(hoje);

            return todas
                .Where(r => 
                {
                    bool isRecorrente = Convert.ToBoolean(r.Recorrente);
                    if (!isRecorrente) return r.DataParaExecucao != null && ((DateTime)r.DataParaExecucao).Date == hoje;

                    if (r.DataInicio != null && ((DateTime)r.DataInicio).Date > hoje) return false;
                    if (r.DataFim != null && ((DateTime)r.DataFim).Date < hoje) return false;

                    if (r.DiasSemana != null)
                    {
                        var rotadias = (eFlagDiaSemana)Convert.ToInt32(r.DiasSemana);
                        return rotadias.HasFlag(flagHoje);
                    }
                    return false;
                })
                .Select(r => new RotaCheckListDTO {
                    Id = r.Id,
                    Codigo = $"RT-{r.Id:000}",
                    Nome = r.Nome,
                    Descricao = Convert.ToBoolean(r.Recorrente) ? $"{r.Nome} (Recorrente)" : $"{r.Nome} (Planejada para {r.DataParaExecucao:dd/MM/yyyy})"
                }).ToList();
        }

        public List<RotaCheckListDTO> ObterRotasAcompanhante(int servidorId)
        {
            var sql = @"
                SELECT r.Id, r.Descricao as Nome, r.Recorrente, r.DataParaExecucao, r.DataInicio, r.DataFim, r.DiasSemana
                FROM rota r
                INNER JOIN rotaprofissional rp ON rp.RotaId = r.Id
                WHERE r.Situacao = 1 AND rp.ServidorId = @ID"; // 1 = Ativa

            var todas = _repositorioRota.ConsultaDapper<dynamic>(sql, new { @ID = servidorId }).ToList();
            var hoje = DateTime.Now.Date;
            var flagHoje = ObterFlagHoje(hoje);

            return todas
                .Where(r => 
                {
                    bool isRecorrente = Convert.ToBoolean(r.Recorrente);
                    if (!isRecorrente) return r.DataParaExecucao != null && ((DateTime)r.DataParaExecucao).Date == hoje;

                    if (r.DataInicio != null && ((DateTime)r.DataInicio).Date > hoje) return false;
                    if (r.DataFim != null && ((DateTime)r.DataFim).Date < hoje) return false;

                    if (r.DiasSemana != null)
                    {
                        var rotadias = (eFlagDiaSemana)Convert.ToInt32(r.DiasSemana);
                        return rotadias.HasFlag(flagHoje);
                    }
                    return false;
                })
                .Select(r => new RotaCheckListDTO {
                    Id = r.Id,
                    Codigo = $"RT-{r.Id:000}",
                    Nome = r.Nome,
                    Descricao = Convert.ToBoolean(r.Recorrente) ? $"{r.Nome} (Recorrente)" : $"{r.Nome} (Planejada para {r.DataParaExecucao:dd/MM/yyyy})"
                }).ToList();
        }

        public dynamic ObterUltimaLocalizacao(int rotaId)
        {
            var sql = "SELECT Latitude, Longitude, DataHora FROM localizacaorota WHERE RotaId = @ID ORDER BY Id DESC LIMIT 1";
            return _repositorioLocalizacao.ConsultaDapper<dynamic>(sql, new { @ID = rotaId }).FirstOrDefault();
        }

        public IEnumerable<dynamic> ObterTrajeto(int rotaId, DateTime data)
        {
            var sql = "SELECT Latitude, Longitude, DataHora FROM localizacaorota WHERE RotaId = @ID AND DATE(DataHora) = @DATA ORDER BY DataHora";
            return _repositorioLocalizacao.ConsultaDapper<dynamic>(sql, new { @ID = rotaId, @DATA = data.Date });
        }

        public List<VeiculoCheckListDTO> ObterVeiculosChecklist(int rotaId)
        {
            var sqlVeiculos = @"
                SELECT v.Id, v.Modelo, v.Placa, v.Cor
                FROM veiculo v
                INNER JOIN rotaveiculo rv ON rv.VeiculoId = v.Id
                WHERE rv.RotaId = @RotaId";
            
            var veiculos = _repositorioRota.ConsultaDapper<dynamic>(sqlVeiculos, new { @RotaId = rotaId }).ToList();

            var resultado = new List<VeiculoCheckListDTO>();
            foreach (var v in veiculos)
            {
                var sqlItems = "SELECT Id, Descricao FROM checklistitem WHERE VeiculoId = @VeiculoId AND Ativo = 1";
                var items = _repositorioRota.ConsultaDapper<CheckListItemDTO>(sqlItems, new { @VeiculoId = v.Id }).ToList();

                resultado.Add(new VeiculoCheckListDTO {
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

        public void SalvarChecklist(ChecklistPostDTO dto, int motoristaId)
        {
            var sqlChecklist = @"INSERT INTO checklistexecucao (OrganizacaoId, VeiculoId, MotoristaId, RotaId, DataHora) 
                                 VALUES ((SELECT OrganizacaoId FROM veiculo WHERE Id = @VeiculoId), @VeiculoId, @MotoristaId, @RotaId, @Agora);
                                 SELECT LAST_INSERT_ID();";
            
            var id = _repositorioChecklistExec.ConsultaDapper<int>(sqlChecklist, new { 
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
        }

        public RotaExecucaoDTO IniciarRota(IniciarRotaAppDTO dto, int motoristaId)
        {
            var sqlCheck = "SELECT Id FROM rotaexecucao WHERE RotaId = @RotaId AND DataHoraFim IS NULL LIMIT 1";
            if (_repositorioExecucao.ConsultaDapper<int>(sqlCheck, new { @RotaId = dto.RotaId }).Any())
                throw new ApplicationException("Esta rota já está em execução.");

            var sqlInsert = @"INSERT INTO rotaexecucao (OrganizacaoId, RotaId, VeiculoId, MotoristaId, UsuarioIdInicio, DataHoraInicio)
                              VALUES ((SELECT OrganizacaoId FROM rota WHERE Id = @RotaId), @RotaId, @VeiculoId, @MotoristaId, @MotoristaId, @Agora);
                              SELECT LAST_INSERT_ID();";
            
            var id = _repositorioExecucao.ConsultaDapper<int>(sqlInsert, new {
                @RotaId = dto.RotaId,
                @VeiculoId = dto.VeiculoId,
                @MotoristaId = motoristaId,
                @Agora = DateTime.Now
            }).First();

            return ObterRotaEmAndamentoAux(id, dto.RotaId);
        }

        public RotaExecucaoDTO? ObterRotaEmAndamento(int motoristaId)
        {
            var sqlAtiva = "SELECT Id, RotaId FROM rotaexecucao WHERE MotoristaId = @MotoristaId AND DataHoraFim IS NULL LIMIT 1";
            var exec = _repositorioExecucao.ConsultaDapper<dynamic>(sqlAtiva, new { @MotoristaId = motoristaId }).FirstOrDefault();

            if (exec == null) return null;

            return ObterRotaEmAndamentoAux((int)exec.Id, (int)exec.RotaId);
        }

        private RotaExecucaoDTO ObterRotaEmAndamentoAux(int execucaoId, int rotaId)
        {
            var sqlRota = @"SELECT r.Id, r.Descricao, r.PermitePausa, r.QuantidadePausas, 
                                   uo.Nome as NomeUnidadeOrigem, ud.Nome as NomeUnidadeDestino,
                                   e.OrigemEntregue, e.OrigemObservacao, e.DestinoEntregue, e.DestinoObservacao, e.HistoricoPausas
                            FROM rotaexecucao e
                            INNER JOIN rota r ON r.Id = e.RotaId
                            LEFT JOIN unidadeorganizacional uo ON r.UnidadeOrigemId = uo.Id 
                            LEFT JOIN unidadeorganizacional ud ON r.UnidadeDestinoId = ud.Id 
                            WHERE e.Id = @ExecId";
            var rota = _repositorioRota.ConsultaDapper<dynamic>(sqlRota, new { @ExecId = execucaoId }).First();

            var sqlParadas = "SELECT Id, Endereco, Latitude, Longitude, Link, Entregue, Observacao FROM paradarota WHERE RotaId = @RotaId";
            var paradas = _repositorioParada.ConsultaDapper<ParadaRotaDTO>(sqlParadas, new { @RotaId = rotaId }).ToList();

            var pausada = false;
            var emAndamento = true;
            var pausasTotais = 0;

            if (!string.IsNullOrEmpty((string)rota.HistoricoPausas))
            {
                var historico = JsonConvert.DeserializeObject<List<RegistoPausaExecucao>>((string)rota.HistoricoPausas);
                if (historico != null)
                {
                    pausasTotais = historico.Count;
                    pausada = historico.Any(p => !p.DataHoraFim.HasValue);
                }
            }

            return new RotaExecucaoDTO {
                Id = execucaoId,
                RotaId = rotaId,
                Descricao = rota.Descricao,
                EmAndamento = emAndamento,
                PermitePausa = Convert.ToBoolean(rota.PermitePausa),
                QuantidadePausas = Convert.ToInt32(rota.QuantidadePausas),
                QuantidadePausasRealizadas = pausasTotais,
                EstaPausada = pausada,
                NomeUnidadeOrigem = rota.NomeUnidadeOrigem,
                OrigemEntregue = rota.OrigemEntregue == null ? (bool?)null : Convert.ToBoolean(rota.OrigemEntregue),
                OrigemObservacao = rota.OrigemObservacao,
                NomeUnidadeDestino = rota.NomeUnidadeDestino,
                DestinoEntregue = rota.DestinoEntregue == null ? (bool?)null : Convert.ToBoolean(rota.DestinoEntregue),
                DestinoObservacao = rota.DestinoObservacao,
                Paradas = paradas
            };
        }

        public void EncerrarRota(EncerrarRotaAppDTO dto)
        {
            var sql = "UPDATE rotaexecucao SET DataHoraFim = @Agora, Observacao = @Obs WHERE Id = @Id";
            _repositorioExecucao.ExecutarComando(sql, new { @Agora = DateTime.Now, @Obs = dto.Observacao, @Id = dto.RotaExecucaoId });
        }

        public void ConfirmarParada(ConfirmarParadaAppDTO dto)
        {
            if (dto.ParadaId == -1) // Origem
            {
                var sqlOrigem = "UPDATE rotaexecucao SET OrigemEntregue = @Entregue, OrigemObservacao = @Obs WHERE Id = @Id";
                _repositorioExecucao.ExecutarComando(sqlOrigem, new { 
                    @Entregue = dto.Entregue ?? false, 
                    @Obs = dto.Observacao, 
                    @Id = dto.RotaExecucaoId 
                });
            }
            else if (dto.ParadaId == -2) // Destino
            {
                var sqlDestino = "UPDATE rotaexecucao SET DestinoEntregue = @Entregue, DestinoObservacao = @Obs WHERE Id = @Id";
                _repositorioExecucao.ExecutarComando(sqlDestino, new { 
                    @Entregue = dto.Entregue ?? false, 
                    @Obs = dto.Observacao, 
                    @Id = dto.RotaExecucaoId 
                });
            }
            else
            {
                var sql = "UPDATE paradarota SET Entregue = @Entregue, Observacao = @Obs, ConcluidoEm = @Agora WHERE Id = @Id";
                _repositorioParada.ExecutarComando(sql, new { 
                    @Entregue = dto.Entregue ?? false, 
                    @Obs = dto.Observacao, 
                    @Agora = DateTime.Now, 
                    @Id = dto.ParadaId 
                });
            }
        }

        public void SalvarPonto(PostLocalizacaoExecucaoDTO dto)
        {
            var sql = @"INSERT INTO localizacaorota (RotaId, Latitude, Longitude, DataHora, OrganizacaoId)
                        VALUES ((SELECT RotaId FROM rotaexecucao WHERE Id = @ExecId), @Lat, @Lon, @Data, (SELECT OrganizacaoId FROM rotaexecucao WHERE Id = @ExecId))";
            _repositorioLocalizacao.ExecutarComando(sql, new { 
                @ExecId = dto.RotaExecucaoId, 
                @Lat = dto.Latitude, 
                @Lon = dto.Longitude, 
                @Data = dto.DataHora 
            });
        }

        public void FazerPausa(PausaRotaAppDTO dto)
        {
            var sqlInfo = @"SELECT re.Id, re.HistoricoPausas, r.PermitePausa, r.QuantidadePausas 
                            FROM rotaexecucao re
                            INNER JOIN rota r ON r.Id = re.RotaId
                            WHERE re.Id = @Id";
            var exec = _repositorioExecucao.ConsultaDapper<dynamic>(sqlInfo, new { @Id = dto.RotaExecucaoId }).FirstOrDefault();

            if (exec == null) throw new ApplicationException("Execução não encontrada.");
            if (!(bool)exec.PermitePausa) throw new ApplicationException("A rota não permite pausas.");

            var pausas = string.IsNullOrEmpty((string)exec.HistoricoPausas) ? new List<RegistoPausaExecucao>() : JsonConvert.DeserializeObject<List<RegistoPausaExecucao>>((string)exec.HistoricoPausas);

            if (pausas.Count >= (int)exec.QuantidadePausas)
                throw new ApplicationException("Limite de pausas atingido para esta rota.");

            if (pausas.Any(p => !p.DataHoraFim.HasValue))
                throw new ApplicationException("Já existe uma pausa em andamento.");

            pausas.Add(new RegistoPausaExecucao
            {
                DataHoraInicio = dto.DataHora,
                LatInicio = dto.Latitude ?? "",
                LngInicio = dto.Longitude ?? "",
                Motivo = dto.Motivo ?? "Pausa informada pelo app"
            });

            var sqlUpdate = "UPDATE rotaexecucao SET HistoricoPausas = @Json WHERE Id = @Id";
            _repositorioExecucao.ExecutarComando(sqlUpdate, new { @Json = JsonConvert.SerializeObject(pausas), @Id = dto.RotaExecucaoId });
        }

        public void FinalizarPausa(PausaRotaAppDTO dto)
        {
            var sqlInfo = "SELECT HistoricoPausas FROM rotaexecucao WHERE Id = @Id";
            var json = _repositorioExecucao.ConsultaDapper<string>(sqlInfo, new { @Id = dto.RotaExecucaoId }).FirstOrDefault();

            if (string.IsNullOrEmpty(json)) return;

            var pausas = JsonConvert.DeserializeObject<List<RegistoPausaExecucao>>(json);
            var pausaAtual = pausas.LastOrDefault(p => !p.DataHoraFim.HasValue);

            if (pausaAtual != null)
            {
                pausaAtual.DataHoraFim = dto.DataHora;
                pausaAtual.LatFim = dto.Latitude ?? "";
                pausaAtual.LngFim = dto.Longitude ?? "";

                var sqlUpdate = "UPDATE rotaexecucao SET HistoricoPausas = @Json WHERE Id = @Id";
                _repositorioExecucao.ExecutarComando(sqlUpdate, new { @Json = JsonConvert.SerializeObject(pausas), @Id = dto.RotaExecucaoId });
            }
        }

        public void ReceberLocalizacao(PostLocalizacaoRotaDTO dto)
        {
            var sql = @"INSERT INTO localizacaorota (RotaId, Latitude, Longitude, DataHora, OrganizacaoId)
                        VALUES (@RotaId, @Lat, @Lon, @Data, (SELECT OrganizacaoId FROM rota WHERE Id = @RotaId))";
            _repositorioLocalizacao.ExecutarComando(sql, new { 
                @RotaId = dto.RotaId, 
                @Lat = dto.Latitude, 
                @Lon = dto.Longitude, 
                @Data = dto.DataHora 
            });

            // Lógica de Desvio (Simplificada para Dapper)
            var sqlRotaInfo = "SELECT PolylineOficial, OrganizacaoId FROM rota WHERE Id = @RotaId";
            var rota = _repositorioRota.ConsultaDapper<dynamic>(sqlRotaInfo, new { @RotaId = dto.RotaId }).FirstOrDefault();

            if (rota != null && !string.IsNullOrEmpty((string)rota.PolylineOficial))
            {
                if (double.TryParse(dto.Latitude, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(dto.Longitude, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lon))
                {
                    var driverCoord = new Utilitarios.PolylineUtils.Coordinate { Latitude = lat, Longitude = lon };
                    var polylinePath = Utilitarios.PolylineUtils.Decode((string)rota.PolylineOficial);
                    
                    double minDistance = double.MaxValue;
                    for (int i = 0; i < polylinePath.Count - 1; i++)
                    {
                        var dist = Utilitarios.PolylineUtils.DistanceToSegment(driverCoord, polylinePath[i], polylinePath[i+1]);
                        if (dist < minDistance) minDistance = dist;
                    }

                    if (minDistance > 500)
                    {
                        var sqlDesvio = @"INSERT INTO rotaocorrenciadesvio (RotaId, Latitude, Longitude, DistanciaEmMetros, DataHora, OrganizacaoId)
                                          VALUES (@RotaId, @Lat, @Lon, @Dist, @Data, @OrgId)";
                        _repositorioDesvio.ExecutarComando(sqlDesvio, new { 
                            @RotaId = dto.RotaId, @Lat = dto.Latitude, @Lon = dto.Longitude, @Dist = minDistance, @Data = dto.DataHora, @OrgId = rota.OrganizacaoId 
                        });
                    }
                }
            }
        }

        public int ObterMotoristaIdPorServidor(int servidorId)
        {
            var sql = "SELECT Id FROM motorista WHERE ServidorId = @ServidorId LIMIT 1";
            return _repositorioMotorista.ConsultaDapper<int>(sql, new { @ServidorId = servidorId }).FirstOrDefault();
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
