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
                        {(unidadeId.HasValue ? "and es.Id = @UNIDADEID" : string.Empty)}
                        and r.DataHoraRegistro between @PERIODOFIM and @PERIODOINICIO
                    order by r.DataHoraRegistro desc";

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

                return dto;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}