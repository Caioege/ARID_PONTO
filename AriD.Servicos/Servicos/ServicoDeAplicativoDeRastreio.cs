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
                        s.TipoComprovacaoPontoApp as TipoComprovacaoApp
                    from pessoa p
                    inner join organizacao o on o.Id = p.OrganizacaoId
                    inner join servidor s on s.PessoaId = p.Id
                    where replace(replace(p.Cpf, '.', ''), '-', '') = @USUARIO ";

                if (isAcompanhante)
                {
                    // Acompanhante: Deve estar vinculado a pelo menos uma rota
                    queryAcesso += @" and exists (select 1 from rotaprofissional rp where rp.ServidorId = s.Id) ";
                }
                else
                {
                    // Motorista: Deve ter cadastro ativo na tabela Motorista e senha válida
                    queryAcesso += @" and exists (select 1 from motorista m where m.ServidorId = s.Id and m.Situacao = 0) "; // 0 = Ativo
                    queryAcesso += @" and (IF(s.SenhaPersonalizadaDeAcesso IS NULL, DATE_FORMAT(p.DataDeNascimento, '%d%m%Y') = @SENHA, s.SenhaPersonalizadaDeAcesso = @SENHACRIPTOGRAFADA) = true) ";
                }

                queryAcesso += @" and s.AcessoAoAplicativo = true limit 1";

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
                SELECT r.Id, r.Descricao as Nome, r.Recorrente, r.DataParaExecucao
                FROM rota r
                WHERE r.Situacao = 0 AND r.MotoristaId = @ID"; // 0 = Ativa

            var todas = _repositorioRota.ConsultaDapper<dynamic>(sql, new { @ID = motoristaId }).ToList();
            var hoje = DateTime.Now.Date;

            return todas
                .Where(r => (bool)r.Recorrente || (!r.Recorrente && r.DataParaExecucao != null && ((DateTime)r.DataParaExecucao).Date == hoje))
                .Select(r => new RotaCheckListDTO {
                    Id = r.Id,
                    Codigo = $"RT-{r.Id:000}",
                    Nome = r.Nome,
                    Descricao = r.Recorrente ? "Rota Recorrente" : $"Planejada para {r.DataParaExecucao:dd/MM/yyyy}"
                }).ToList();
        }

        public List<RotaCheckListDTO> ObterRotasAcompanhante(int servidorId)
        {
            var sql = @"
                SELECT r.Id, r.Descricao as Nome, r.Recorrente, r.DataParaExecucao
                FROM rota r
                INNER JOIN rotaprofissional rp ON rp.RotaId = r.Id
                WHERE r.Situacao = 0 AND rp.ServidorId = @ID";

            var todas = _repositorioRota.ConsultaDapper<dynamic>(sql, new { @ID = servidorId }).ToList();
            var hoje = DateTime.Now.Date;

            return todas
                .Where(r => (bool)r.Recorrente || (!r.Recorrente && r.DataParaExecucao != null && ((DateTime)r.DataParaExecucao).Date == hoje))
                .Select(r => new RotaCheckListDTO {
                    Id = r.Id,
                    Codigo = $"RT-{r.Id:000}",
                    Nome = r.Nome,
                    Descricao = r.Recorrente ? "Rota Recorrente" : $"Planejada para {r.DataParaExecucao:dd/MM/yyyy}"
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

            var sqlRota = "SELECT Id, Descricao FROM rota WHERE Id = @RotaId";
            var rota = _repositorioRota.ConsultaDapper<dynamic>(sqlRota, new { @RotaId = dto.RotaId }).First();

            var sqlParadas = "SELECT Id, Endereco, Latitude, Longitude, Link FROM paradarota WHERE RotaId = @RotaId";
            var paradas = _repositorioParada.ConsultaDapper<ParadaRotaDTO>(sqlParadas, new { @RotaId = dto.RotaId }).ToList();

            return new RotaExecucaoDTO {
                Id = id,
                RotaId = rota.Id,
                Descricao = rota.Descricao,
                EmAndamento = true,
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
            var sql = "UPDATE paradarota SET Entregue = @Entregue, Observacao = @Obs, ConcluidoEm = @Agora WHERE Id = @Id";
            _repositorioParada.ExecutarComando(sql, new { 
                @Entregue = dto.Entregue ?? false, 
                @Obs = dto.Observacao, 
                @Agora = DateTime.Now, 
                @Id = dto.ParadaId 
            });
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
    }
}
