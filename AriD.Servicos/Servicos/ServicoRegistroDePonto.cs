using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoRegistroDePonto : Servico<RegistroDePonto>, IServicoRegistroDePonto
    {
        private readonly IRepositorio<RegistroDePonto> _repositorio;
        private readonly IWhatsappService _whatsappService;
        private readonly IEmailService _emailService;

        public ServicoRegistroDePonto(
            IRepositorio<RegistroDePonto> repositorio,
            IWhatsappService whatsappService,
            IEmailService emailService)
            : base(repositorio)
        {
            _repositorio = repositorio;
            _whatsappService = whatsappService;
            _emailService = emailService;
        }

        public async Task ReceberRegistroDeEquipamento(
            RegistroEquipamentoDTO dados)
        {
            try
            {
                var query = @"SELECT
	                            OrganizacaoId as 'Key',
                                Id as 'Value'
                            FROM
                                equipamentodeponto
                            WHERE
                                NumeroDeSerie = @SERIALNUMBER
                            LIMIT 1";

                var dadosEquipamento = _repositorio.ConsultaDapper<KeyValuePair<int?, int?>>(
                    query, 
                    new 
                    {
                        @SERIALNUMBER = dados.SerialNumber
                    }).FirstOrDefault();

                if (dadosEquipamento.Key.HasValue && dadosEquipamento.Value.HasValue)
                {
                    var registroDePonto = new RegistroDePonto
                    {
                        OrganizacaoId = dadosEquipamento.Key.Value,
                        UsuarioEquipamentoId = dados.UsuarioId,
                        EquipamentoDePontoId = dadosEquipamento.Value.Value,
                        DataHoraRecebimento = DateTime.Now,
                        DataHoraRegistro = dados.DataHora,
                        TipoRegistro = (eTipoDeRegistroEquipamento)dados.ModoDeAcesso
                    };

                    _repositorio.Add(registroDePonto);
                    _repositorio.Commit();

                    var envioComprovante = ObtenhaDadosEnvioDeComprovante(dadosEquipamento.Value.Value, dados.UsuarioId);
                    if (envioComprovante != null && envioComprovante.EnvioDeMensagemWhatsAppExperimental)
                    {
                        try
                        {
                            var servidor = new Servidor
                            {
                                OrganizacaoId = envioComprovante.OrganizacaoId,
                                Organizacao = new() { Nome = envioComprovante.OrganizacaoNome, EnvioDeMensagemWhatsAppExperimental = envioComprovante.EnvioDeMensagemWhatsAppExperimental },

                                TelefoneDeContato = envioComprovante.TelefoneDeContato,

                                Pessoa = new() { Nome = envioComprovante.ServidorNome, Cpf = envioComprovante.ServidorCpf }
                            };

                            await _whatsappService.EnviarComprovantePontoAsync(servidor, registroDePonto.Id, dados.DataHora);
                            await _emailService.EnviarComprovantePontoAsync(servidor, registroDePonto.Id, dados.DataHora);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ProcessarMonitoramentoConectividade(List<MonitoramentoConectividadeDTO> dados)
        {
            if (dados == null || !dados.Any()) return;

            var serials = dados.Select(d => d.NumeroSerie).ToList();

            var query = @"
                SELECT 
                    e.Descricao, 
                    e.NumeroDeSerie AS NumeroSerie, 
                    u.Nome AS UnidadeNome,
                    o.Id AS OrganizacaoId,
                    o.Nome AS OrganizacaoNome,
                    o.RecebeNotificacaoConectividade AS OrgRecebe,
                    o.EmailNotificacaoConectividade AS OrgEmail,
                    u.Id AS UnidadeId,
                    u.RecebeNotificacaoConectividade AS UnitRecebe,
                    u.EmailNotificacaoConectividade AS UnitEmail
                FROM equipamentodeponto e
                INNER JOIN unidadeorganizacional u ON u.Id = e.UnidadeOrganizacionalId
                INNER JOIN organizacao o ON o.Id = e.OrganizacaoId
                WHERE e.NumeroDeSerie IN @SERIALS AND e.Ativo = 1";

            var infoEquipamentos = _repositorio.ConsultaDapper<dynamic>(query, new { @SERIALS = serials }).ToList();

            if (!infoEquipamentos.Any()) return;

            // Agrupar por Organização
            var porOrganizacao = infoEquipamentos.GroupBy(x => (int)x.OrganizacaoId);

            foreach (var grupoOrg in porOrganizacao)
            {
                var primeiraInfo = grupoOrg.First();
                bool orgRecebe = Convert.ToBoolean(primeiraInfo.OrgRecebe);
                string orgEmail = primeiraInfo.OrgEmail;
                string orgNome = primeiraInfo.OrganizacaoNome;

                var equipamentosComData = grupoOrg.Select(e => new EquipamentoConectividadeInfo
                {
                    Descricao = e.Descricao,
                    NumeroSerie = e.NumeroSerie,
                    UnidadeNome = e.UnidadeNome,
                    DataHoraUltimoRegistro = dados.FirstOrDefault(d => d.NumeroSerie == (string)e.NumeroSerie)?.DataHoraUltimoRegistro ?? DateTime.MinValue
                }).ToList();

                // Notificação para a Organização (Todos os equipamentos da org)
                if (orgRecebe && !string.IsNullOrEmpty(orgEmail))
                {
                    await _emailService.EnviarNotificacaoConectividadeAsync(orgEmail, orgNome, equipamentosComData);
                }

                // Notificação por Unidade
                var porUnidade = grupoOrg.GroupBy(x => (int)x.UnidadeId);
                foreach (var grupoUnidade in porUnidade)
                {
                    var infoUnidade = grupoUnidade.First();
                    bool unitRecebe = Convert.ToBoolean(infoUnidade.UnitRecebe);
                    string unitEmail = infoUnidade.UnitEmail;
                    string unitNome = infoUnidade.UnidadeNome;

                    if (unitRecebe && !string.IsNullOrEmpty(unitEmail))
                    {
                        var equipamentosDaUnidade = equipamentosComData
                            .Where(e => grupoUnidade.Any(gu => (string)gu.NumeroSerie == e.NumeroSerie))
                            .ToList();

                        await _emailService.EnviarNotificacaoConectividadeAsync(unitEmail, unitNome, equipamentosDaUnidade);
                    }
                }
            }
        }

        public (int Total, List<RegistroDePontoIndexDTO> Itens) ObtenhaListaPaginadaDTO(
            ParametrosDeConsultaRegistroDePonto parametros)
        {
            try
            {
                var fromAndWhere = @"
                            from registrodeponto reg
                            left join equipamentodeponto eq
	                            on eq.Id = reg.EquipamentoDePontoId
                            left join unidadeorganizacional uni
	                            on uni.Id = eq.UnidadeOrganizacionalId
                            left join lotacaounidadeorganizacional lot
	                            on lot.UnidadeOrganizacionalId = uni.Id and lot.MatriculaEquipamento = reg.UsuarioEquipamentoId
                            left join registroaplicativo regap
                                on regap.Id = reg.RegistroAplicativoId
                            left join vinculodetrabalho vin
	                            on vin.Id = coalesce(regap.VinculoDeTrabalhoId, lot.VinculoDeTrabalhoId)
                            left join servidor ser
	                            on ser.Id = vin.ServidorId
                            left join pessoa pes
	                            on pes.Id = ser.PessoaId
                            where
	                            reg.OrganizacaoId = @ORGANIZACAOID
                                and (regap.Id is null or regap.Manual = false)";

                if (parametros.Unidades.Any())
                    fromAndWhere += " and uni.Id in @UNIDADES or exists (select 1 from lotacaounidadeorganizacional lotvin where lotvin.VinculoDeTrabalhoId = vin.Id and lot.UnidadeOrganizacionalId in @UNIDADES) ";

                if (parametros.DepartamentoId.HasValue)
                    fromAndWhere += " and vin.DepartamentoId = @DEPARTAMENTOID ";

                if (!string.IsNullOrEmpty(parametros.Pesquisa))
                {
                    parametros.Pesquisa = parametros.Pesquisa.ToLower();
                    fromAndWhere += @" and (lower(pes.Nome) like concat('%', @PESQUISA, '%')
	                            or reg.UsuarioEquipamentoId like concat('%', @PESQUISA, '%')
                                or vin.Matricula like concat('%', @PESQUISA, '%')
                                or lower(uni.Nome) like concat('%', @PESQUISA, '%')
                                or lower(eq.Descricao) like concat('%', @PESQUISA, '%'))";
                }

                var count = $"select count(1) {fromAndWhere}";
                var select = $@"select
	                            reg.Id as Id,
                                eq.Id as EquipamentoId,
                                eq.Descricao as EquipamentoDescricao,
                                reg.DataHoraRegistro as DataHoraRegistro,
                                reg.DataHoraRecebimento as DataHoraRecebimento,
                                reg.UsuarioEquipamentoId as IdEquipamento,
                                pes.Nome as PessoaNome,
                                uni.Id as UnidadeOrganizacionalId,
                                uni.Nome as UnidadeOrganizacionalNome,
                                if (reg.RegistroAplicativoId is not null, 'Aplicativo', 'Equipamento de Ponto') as Origem
                            {fromAndWhere}
                            order by reg.DataHoraRegistro, reg.DataHoraRecebimento, pes.Nome
                        limit @LIMIT
                        offset @OFFSET";

                var parametrosConsultaDapper = new
                {
                    @ORGANIZACAOID = parametros.OrganizacaoId,
                    @UNIDADES = parametros.Unidades,
                    @PESQUISA = parametros.Pesquisa,
                    @LIMIT = parametros.TotalPorPagina,
                    @OFFSET = (parametros.TotalPorPagina * (parametros.Pagina- 1)),
                    @DEPARTAMENTOID = parametros.DepartamentoId
                };

                var total = _repositorio.ConsultaDapper<int?>(count, parametrosConsultaDapper).FirstOrDefault() ?? 0;
                var itens = total > 0 ?
                    _repositorio.ConsultaDapper<RegistroDePontoIndexDTO>(select, parametrosConsultaDapper) :
                    new List<RegistroDePontoIndexDTO>();

                return (total, itens);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private EnvioDeComprovanteDTO ObtenhaDadosEnvioDeComprovante(int equipamentoId, string matriculaEquipamento)
        {
            var query =
                @"select
	                o.Id as OrganizacaoId,
                    o.Nome as OrganizacaoNome,
                    o.EnvioDeMensagemWhatsAppExperimental,
                    s.Id as ServidorId,
                    p.Nome as ServidorNome,
                    p.Cpf  as ServidorCpf,
                    s.TelefoneDeContato
                from equipamentodeponto e
                inner join lotacaounidadeorganizacional l
	                on l.UnidadeOrganizacionalId = e.UnidadeOrganizacionalId
                inner join vinculodetrabalho v
	                on v.Id = l.VinculoDeTrabalhoId
                inner join servidor s
	                on s.Id = v.ServidorId
                inner join pessoa p
	                on p.Id = s.PessoaId
                inner join organizacao o
	                on o.Id = e.OrganizacaoId
                where
	                e.Id = @EQUIPAMENTOID
                    and l.MatriculaEquipamento = @MATRICULAEQUIPAMENTO
                limit 1";

            return _repositorio.ConsultaDapper<EnvioDeComprovanteDTO>(query, new
            {
                @EQUIPAMENTOID = equipamentoId,
                @MATRICULAEQUIPAMENTO = matriculaEquipamento
            }).FirstOrDefault();
        }
    }
}