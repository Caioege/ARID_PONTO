using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeDashboard : IServicoDeDashboard
    {
        private readonly IRepositorio<Aluno> _repositorio;

        public ServicoDeDashboard(IRepositorio<Aluno> repositorio)
        {
            _repositorio = repositorio;
        }

        public DashboardDTO ObtenhaDashboardDTO(
            int redeDeEnsinoId,
            int? escolaId)
        {
            try
            {
                var dto = new DashboardDTO();

                var query =
                    $@"select
	                    count(1)
                    from aluno
                    where
	                    RedeDeEnsinoId = @REDEDEENSINOID
                        and EscolaId is not null
                        {(escolaId.HasValue ? "and EscolaId = @ESCOLAID" : string.Empty)}";

                dto.TotalDeAlunosMatriculados = _repositorio.ConsultaDapper<int?>(query, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId
                }).FirstOrDefault() ?? 0;

                query =
                    $@"select
	                    count(r.Id)
                    from registrodeponto r
                    inner join equipamentodefrequencia e
	                    on e.Id = r.EquipamentoDeFrequenciaId
                    where
	                    r.RedeDeEnsinoId = @REDEDEENSINOID
                        {(escolaId.HasValue ? "and e.EscolaId = @ESCOLAID" : string.Empty)}
                        and date(r.DataHoraRegistro) = @DATAATUAL";

                dto.TotalDeRegistrosHoje = _repositorio.ConsultaDapper<int?>(query, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId,
                    @DATAATUAL = DateTime.Today
                }).FirstOrDefault() ?? 0;

                query =
                    $@"select
	                    count(1)
                    from equipamentodefrequencia
                    where
	                    RedeDeEnsinoId = @REDEDEENSINOID
                        {(escolaId.HasValue ? "and EscolaId = @ESCOLAID" : string.Empty)}
                        and Ativo = true";

                dto.TotalDeEquipamentosAtivos = _repositorio.ConsultaDapper<int?>(query, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId
                }).FirstOrDefault() ?? 0;

                query =
                    $@"select
	                    es.Nome as EscolaNome,
                        p.Nome as PessoaNome,
                        r.UsuarioEquipamentoId as IdEquipamento,
                        concat(e.Descricao, ' - ', e.NumeroDeSerie) as Equipamento,
                        r.DataHoraRegistro
                    from registrodeponto r
                    inner join equipamentodefrequencia e
	                    on e.Id = r.EquipamentoDeFrequenciaId
                    inner join escola es
	                    on es.Id = e.EscolaId
                    left join aluno a
	                    on a.EscolaId = es.Id and a.IdEquipamento = r.UsuarioEquipamentoId
                    left join pessoa p
	                    on p.Id = a.PessoaId
                    where
	                    r.RedeDeEnsinoId = @REDEDEENSINOID
                        {(escolaId.HasValue ? "and es.Id = @ESCOLAID" : string.Empty)}
                        and r.DataHoraRegistro between @PERIODOINICIO and @PERIODOFIM
                    order by r.DataHoraRegistro desc";

                dto.UltimosRegistrosRecebidos = _repositorio.ConsultaDapper<DashboardRegistroEquipamentoDTO>(query, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId,
                    @PERIODOINICIO = DateTime.Now,
                    @PERIODOFIM = DateTime.Now.Subtract(TimeSpan.FromMinutes(30)),
                });

                query =
                    $@"SELECT
                        r.DataHoraRegistro
                    FROM registrodeponto r
                    INNER JOIN equipamentodefrequencia e ON e.Id = r.EquipamentoDeFrequenciaId
                    WHERE
                        r.RedeDeEnsinoId = @REDEDEENSINOID
                        {(escolaId.HasValue ? "AND e.EscolaId = @ESCOLAID" : string.Empty)}
                        AND r.DataHoraRegistro >= DATE_SUB(NOW(), INTERVAL 5 HOUR)
                    LIMIT 5";

                var dadosHora = _repositorio.ConsultaDapper<DateTime>(query, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId
                }).GroupBy(c => c.ToString("HH:mm"));

                if (dadosHora.Any())
                    dto.RegistrosPorHorario = new Tuple<string[], int[]>(
                        dadosHora.Select(c => c.Key).ToArray(), 
                        dadosHora.Select(c => c.Count()).ToArray());

                if (escolaId.HasValue)
                {
                    query =
                        $@"select
	                        e.Descricao as 'Key',
                            count(r.Id) as 'Value'
                        from registrodeponto r
                        inner join equipamentodefrequencia e
	                        on e.Id = r.EquipamentoDeFrequenciaId
                        where
	                        r.RedeDeEnsinoId = @REDEDEENSINOID
                            and e.EscolaId = @ESCOLAID
                            and date(r.DataHoraRegistro) = @DATAATUAL
                        group by e.Id
                        order by e.Descricao";

                    var dadosEquipamentos = _repositorio.ConsultaDapper<KeyValuePair<string, int>>(query, new
                    {
                        @REDEDEENSINOID = redeDeEnsinoId,
                        @ESCOLAID = escolaId,
                        @DATAATUAL = DateTime.Today
                    });

                    dto.RegistrosPorEquipamento = new Tuple<string[], int[]>(
                        dadosEquipamentos.Select(c => c.Key).ToArray(),
                        dadosEquipamentos.Select(c => c.Value).ToArray());
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