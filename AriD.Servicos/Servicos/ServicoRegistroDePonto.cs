using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AriD.Servicos.Servicos
{
    public class ServicoRegistroDePonto : Servico<RegistroDePonto>, IServicoRegistroDePonto
    {
        private readonly IRepositorio<RegistroDePonto> _repositorio;

        public ServicoRegistroDePonto(
            IRepositorio<RegistroDePonto> repositorio)
            : base(repositorio)
        {
            _repositorio = repositorio;
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
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (int Total, List<RegistroDePontoIndexDTO> Itens) ObtenhaListaPaginadaDTO(
            ParametrosDeConsultaRegistroDePonto parametros)
        {
            try
            {
                var fromAndWhere = @"
                            from registrodeponto reg
                            inner join equipamentodeponto eq
	                            on eq.Id = reg.EquipamentoDePontoId
                            inner join unidadeorganizacional uni
	                            on uni.Id = eq.UnidadeOrganizacionalId
                            left join lotacaounidadeorganizacional lot
	                            on lot.UnidadeOrganizacionalId = uni.Id and lot.MatriculaEquipamento = reg.UsuarioEquipamentoId
                            left join vinculodetrabalho vin
	                            on vin.Id = lot.VinculoDeTrabalhoId
                            left join servidor ser
	                            on ser.Id = vin.ServidorId
                            left join pessoa pes
	                            on pes.Id = ser.PessoaId
                            where
	                            reg.OrganizacaoId = @ORGANIZACAOID";

                if (parametros.Unidades.Any())
                    fromAndWhere += " and uni.Id in @UNIDADES";

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
                                uni.Nome as UnidadeOrganizacionalNome
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
    }
}