using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeDashboard : IServicoDeDashboard
    {
        private readonly IRepositorio<Servidor> _repositorio;

        public ServicoDeDashboard(IRepositorio<Servidor> repositorio)
        {
            _repositorio = repositorio;
        }

        public DashboardDTO ObtenhaDashboardDTO(
            int organizacaoId,
            int? unidadeId)
        {
            try
            {
                var dto = new DashboardDTO();

                var query =
                    $@"select
	                    count(1)
                    from vinculodetrabalho v
                    where
	                    v.OrganizacaoId = @ORGANIZACAOID
                        and v.Situacao = @SITUACAOATIVA
                        {(unidadeId.HasValue ? "and exists (select 1 from lotacaounidadeorganizacional l where l.UnidadeOrganizacionalId = @UNIDADEID and l.VinculoDeTrabalhoId = v.Id and coalesce(l.Saida, now()) >= now())" : string.Empty)}";

                dto.TotalDeContratosAtivos = _repositorio.ConsultaDapper<int?>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @UNIDADEID = unidadeId,
                    @SITUACAOATIVA = eSituacaoVinculoDeTrabalho.Normal
                }).FirstOrDefault() ?? 0;

                query =
                    $@"select
	                    count(r.Id)
                    from registrodeponto r
                    inner join equipamentodeponto e
	                    on e.Id = r.EquipamentoDePontoId
                    where
	                    r.OrganizacaoId = @ORGANIZACAOID
                        {(unidadeId.HasValue ? "and e.UnidadeOrganizacionalId = @UNIDADEID" : string.Empty)}
                        and date(r.DataHoraRegistro) = @DATAATUAL";

                dto.TotalDeRegistrosHoje = _repositorio.ConsultaDapper<int?>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @UNIDADEID = unidadeId,
                    @DATAATUAL = DateTime.Today
                }).FirstOrDefault() ?? 0;

                query =
                    $@"select
	                    count(1)
                    from equipamentodeponto
                    where
	                    OrganizacaoId = @ORGANIZACAOID
                        {(unidadeId.HasValue ? "and UnidadeOrganizacionalId = @UNIDADEID" : string.Empty)}
                        and Ativo = true";

                dto.TotalDeEquipamentosAtivos = _repositorio.ConsultaDapper<int?>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @UNIDADEID = unidadeId
                }).FirstOrDefault() ?? 0;

                query =
                    $@"select
                        dados.EscolaNome,
                        dados.PessoaNome,
                        dados.IdEquipamento,
                        dados.Equipamento,
                        dados.DataHoraRegistro as DataHora
                    from 
                    ((select
	                    es.Nome as EscolaNome,
                        p.Nome as PessoaNome,
                        r.UsuarioEquipamentoId as IdEquipamento,
                        concat(e.Descricao, ' - ', e.NumeroDeSerie) as Equipamento,
                        r.DataHoraRegistro
                    from registrodeponto r
                    inner join equipamentodeponto e
	                    on e.Id = r.equipamentodepontoId
                    inner join unidadeorganizacional es
	                    on es.Id = e.UnidadeOrganizacionalId
                    left join lotacaounidadeorganizacional lo
                        on lo.UnidadeOrganizacionalId = e.UnidadeOrganizacionalId
                        and lo.MatriculaEquipamento = r.UsuarioEquipamentoId
                    left join vinculodetrabalho v
                        on v.Id = lo.VinculoDeTrabalhoId
                    left join servidor s
                        on s.Id = v.ServidorId
                    left join pessoa p
	                    on p.Id = s.PessoaId
                    where
	                    r.OrganizacaoId = @ORGANIZACAOID
                        and r.RegistroAplicativoId is null
                        {(unidadeId.HasValue ? "and es.Id = @UNIDADEID" : string.Empty)}
                        and r.DataHoraRegistro between @PERIODOFIM and @PERIODOINICIO)
                    union
                    (select
                        '' as EscolaNome,
                        p.Nome as PessoaNome,
                        '' as IdEquipamento,
                        'Aplicativo' as Equipamento,
                        r.DataHoraRegistro
                    from registrodeponto r
                    inner join registroaplicativo ra
	                    on ra.Id = r.RegistroAplicativoId
                    inner join vinculodetrabalho v
	                    on v.Id = ra.VinculoDeTrabalhoId
                    inner join servidor s
	                    on s.Id = v.ServidorId
                    inner join  pessoa p
	                    on p.Id = s.PessoaId
                    where
	                    r.OrganizacaoId = @ORGANIZACAOID
                        and r.DataHoraRegistro between @PERIODOFIM and @PERIODOINICIO
                        {(unidadeId.HasValue ? "and exists (select 1 from lotacaounidadeorganizacional l where l.VinculoDeTrabalhoId = ra.VinculoDeTrabalhoId and l.UnidadeOrganizacionalId = @UNIDADEID)" : string.Empty)})) as dados
                    order by dados.DataHoraRegistro desc";

                dto.UltimosRegistrosRecebidos = _repositorio.ConsultaDapper<DashboardRegistroEquipamentoDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @UNIDADEID = unidadeId,
                    @PERIODOINICIO = DateTime.Now,
                    @PERIODOFIM = DateTime.Now.Subtract(TimeSpan.FromMinutes(30)),
                });

                query =
                    $@"SELECT
                        r.DataHoraRegistro
                    FROM registrodeponto r
                    INNER JOIN equipamentodeponto e ON e.Id = r.equipamentodepontoId
                    WHERE
                        r.OrganizacaoId = @ORGANIZACAOID
                        {(unidadeId.HasValue ? "AND e.UnidadeOrganizacionalId = @UNIDADEID" : string.Empty)}
                        AND r.DataHoraRegistro >= DATE_SUB(NOW(), INTERVAL 5 HOUR)
                    LIMIT 5";

                var dadosHora = _repositorio.ConsultaDapper<DateTime>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @UNIDADEID = unidadeId
                }).GroupBy(c => c.ToString("HH:mm"));

                if (dadosHora.Any())
                    dto.RegistrosPorHorario = new Tuple<string[], int[]>(
                        dadosHora.Select(c => c.Key).ToArray(),
                        dadosHora.Select(c => c.Count()).ToArray());

                if (unidadeId.HasValue)
                {
                    query =
                        $@"select
	                        e.Descricao as 'Key',
                            count(r.Id) as 'Value'
                        from registrodeponto r
                        inner join equipamentodeponto e
	                        on e.Id = r.equipamentodepontoId
                        where
	                        r.OrganizacaoId = @ORGANIZACAOID
                            and e.UnidadeOrganizacionalId = @UNIDADEID
                            and date(r.DataHoraRegistro) = @DATAATUAL
                        group by e.Id
                        order by e.Descricao";

                    var dadosEquipamentos = _repositorio.ConsultaDapper<KeyValuePair<string, int>>(query, new
                    {
                        @ORGANIZACAOID = organizacaoId,
                        @UNIDADEID = unidadeId,
                        @DATAATUAL = DateTime.Today
                    });

                    dto.RegistrosPorEquipamento = new Tuple<string[], int[]>(
                        dadosEquipamentos.Select(c => c.Key).ToArray(),
                        dadosEquipamentos.Select(c => c.Value).ToArray());
                }

                if (!unidadeId.HasValue)
                {
                    query = @"
                        SELECT 
                            v.Id as VeiculoId,
                            v.Placa,
                            v.Modelo,
                            m.KmProximaManutencao,
                            m.DataVencimentoManutencao,
                            v.QuilometragemAtual
                        FROM veiculo v
                        LEFT JOIN (
                            SELECT VeiculoId, MAX(Id) as MaxId
                            FROM manutencaoveiculo
                            GROUP BY VeiculoId
                        ) max_m ON max_m.VeiculoId = v.Id
                        INNER JOIN manutencaoveiculo m ON m.Id = max_m.MaxId
                        WHERE 
                            v.OrganizacaoId = @ORGANIZACAOID 
                            AND v.Status NOT IN (3, 4)";

                    var rawAlertas = _repositorio.ConsultaDapper<dynamic>(query, new { @ORGANIZACAOID = organizacaoId }).ToList();

                    foreach (var item in rawAlertas)
                    {
                        bool precisaAlerta = false;
                        string motivo = "";
                        string tipoAlerta = "";
                        string expiracaoInfo = "";
                        
                        if (item.KmProximaManutencao != null && item.QuilometragemAtual > 0)
                        {
                            int kmRestante = (int)item.KmProximaManutencao - (int)item.QuilometragemAtual;
                            if (kmRestante <= 1000 && kmRestante > 0)
                            {
                                precisaAlerta = true;
                                motivo = "KM Próxima";
                                tipoAlerta = "Media";
                                expiracaoInfo = $"Restam {kmRestante} KM";
                            }
                            else if (kmRestante <= 0)
                            {
                                precisaAlerta = true;
                                motivo = "KM Atrasada";
                                tipoAlerta = "Alta";
                                expiracaoInfo = $"Atrasado em {-kmRestante} KM";
                            }
                        }
                        
                        if (!precisaAlerta || tipoAlerta != "Alta")
                        {
                            if (item.DataVencimentoManutencao != null)
                            {
                                var dataVencimento = (DateTime)item.DataVencimentoManutencao;
                                var diasRestantes = (dataVencimento - DateTime.Now.Date).TotalDays;
                                
                                if (diasRestantes <= 15 && diasRestantes > 0)
                                {
                                    precisaAlerta = true;
                                    if (tipoAlerta != "Alta") {
                                        motivo = "Vencimento Próximo";
                                        tipoAlerta = "Media";
                                        expiracaoInfo = $"Vence em {diasRestantes:0} dias";
                                    }
                                }
                                else if (diasRestantes <= 0)
                                {
                                    precisaAlerta = true;
                                    motivo = "Data Atrasada";
                                    tipoAlerta = "Alta";
                                    expiracaoInfo = $"Atrasado {(int)-diasRestantes} dias";
                                }
                            }
                        }
                        
                        if (precisaAlerta)
                        {
                            dto.AlertasDeManutencao.Add(new AlertaManutencaoDTO
                            {
                                VeiculoId = item.VeiculoId,
                                Placa = item.Placa,
                                Modelo = item.Modelo,
                                Motivo = motivo,
                                TipoAlerta = tipoAlerta,
                                ExpiracaoInfo = expiracaoInfo
                            });
                        }
                    }

                    // Order by priority (Alta first) and then by motive
                    dto.AlertasDeManutencao = dto.AlertasDeManutencao.OrderBy(a => a.TipoAlerta == "Alta" ? 0 : 1).ToList();
                }

                return dto;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}