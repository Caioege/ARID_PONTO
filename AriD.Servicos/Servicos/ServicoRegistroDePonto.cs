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
	                            RedeDeEnsinoId as 'Key',
                                Id as 'Value'
                            FROM
                                EquipamentoDeFrequencia
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
                        RedeDeEnsinoId = dadosEquipamento.Key.Value,
                        UsuarioEquipamentoId = dados.UsuarioId,
                        EquipamentoDeFrequenciaId = dadosEquipamento.Value.Value,
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
                            inner join EquipamentoDeFrequencia eq
	                            on eq.Id = reg.EquipamentoDeFrequenciaId
                            inner join escola uni
	                            on uni.Id = eq.EscolaId
                            left join aluno alu
	                            on alu.EscolaId = uni.Id and alu.IdEquipamento = reg.UsuarioEquipamentoId
                            left join pessoa pes
	                            on pes.Id = alu.PessoaId
                            where
	                            reg.RedeDeEnsinoId = @redeDeEnsinoID";

                if (parametros.EscolaId.HasValue)
                    fromAndWhere += " and uni.Id = @ESCOLAID";

                if (!string.IsNullOrEmpty(parametros.Pesquisa))
                {
                    parametros.Pesquisa = parametros.Pesquisa.ToLower();
                    fromAndWhere += @" and (lower(pes.Nome) like concat('%', @PESQUISA, '%')
	                            or reg.UsuarioEquipamentoId like concat('%', @PESQUISA, '%')
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
                                uni.Id as EscolaId,
                                uni.Nome as EscolaNome
                            {fromAndWhere}
                            order by reg.DataHoraRegistro, reg.DataHoraRecebimento, pes.Nome
                        limit @LIMIT
                        offset @OFFSET";

                var parametrosConsultaDapper = new
                {
                    @redeDeEnsinoID = parametros.RedeDeEnsinoId,
                    @ESCOLAID = parametros.EscolaId,
                    @PESQUISA = parametros.Pesquisa,
                    @LIMIT = parametros.TotalPorPagina,
                    @OFFSET = (parametros.TotalPorPagina * (parametros.Pagina- 1))
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
