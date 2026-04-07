using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeServidor : IServicoDeServidor
    {
        private readonly IRepositorio<Servidor> _repositorio;

        public ServicoDeServidor(
            IRepositorio<Servidor> repositorio)
        {
            _repositorio = repositorio;
        }

        public void Dispose()
        {
        }

        public void ExecuteAcaoEmLote(int acao, string motivo, SessaoDTO sessao)
        {
            try
            {
                var campo = string.Empty;
                var valor = acao % 2 == 0 ? "false" : "true";

                if (acao >= 1 && acao <= 8)
                {
                    switch (acao)
                    {
                        case 1:
                        case 2:
                            campo = "AcessoAoAplicativo";
                            break;
                        case 3:
                        case 4:
                            campo = "RegistroDePontoNoAplicativo";
                            break;
                        case 5:
                        case 6:
                            campo = "RegistroManualNoAplicativo";
                            break;
                        case 7:
                        case 8:
                            campo = "RegistroDeAtestadoNoAplicativo";
                            break;
                    }

                    if (!string.IsNullOrEmpty(campo))
                    {
                        var comando =
                            $@"update servidor s
                                set s.{campo} = {valor}
                                where
                                    s.OrganizacaoId = @ORGANIZACAOID ";

                        if (sessao.DepartamentoId.HasValue)
                            comando += " and exists (select 1 from vinculodetrabalho v where v.DepartamentoId = @DEPARTAMENTOID and v.ServidorId = s.Id)";

                        if (sessao.UnidadeOrganizacionais.Any())
                            comando += " and exists (select 1 from vinculodetrabalho v inner join lotacaounidadeorganizacional l on l.VinculoDeTrabalhoId = v.Id where l.UnidadeOrganizacionalId in (@UNIDADES) and v.ServidorId = s.Id)";

                        _repositorio.ConsultaDapper<int>(comando, new
                        {
                            @ORGANIZACAOID = sessao.OrganizacaoId,
                            @DEPARTAMENTOID = sessao.DepartamentoId,
                            @UNIDADES = sessao.UnidadeOrganizacionais
                        });
                    }
                }
                else if (acao >= 9 && acao <= 11)
                {
                    if (string.IsNullOrWhiteSpace(motivo))
                        throw new ApplicationException("Motivo é obrigatório para alterar a configuração de segurança em lote.");

                    int novoTipo = 0; // 9 = Nenhuma
                    if (acao == 10) novoTipo = 1; // 10 = ApenasSelfie
                    if (acao == 11) novoTipo = 2; // 11 = LivenessFacial

                    // Usa Dapper para efetuar a gravação de banco performática, incluindo o Historico.
                    // Isso pressupõe que o insert pega todos os servidores da organização
                    // que atualmente possuem Tipo diferente do que se quer setar.
                    var where = " where OrganizacaoId = @ORGANIZACAOID and TipoComprovacaoPontoApp <> @NOVOTIPO ";
                    if (sessao.DepartamentoId.HasValue)
                        where += " and exists (select 1 from vinculodetrabalho v where v.DepartamentoId = @DEPARTAMENTOID and v.ServidorId = s.Id)";

                    if (sessao.UnidadeOrganizacionais.Any())
                        where += " and exists (select 1 from vinculodetrabalho v inner join lotacaounidadeorganizacional l on l.VinculoDeTrabalhoId = v.Id where l.UnidadeOrganizacionalId in (@UNIDADES) and v.ServidorId = s.Id)";

                    var cmdHistorico =
                        $@"insert into historicoconfiguracaoappservidor 
                            (ServidorId, TipoComprovacaoAnterior, TipoComprovacaoNova, Motivo, DataAlteracao, UsuarioAlteracaoId, OrganizacaoId, InativoDesde, Inativo)
                           select 
                            Id as ServidorId,
                            TipoComprovacaoPontoApp as TipoComprovacaoAnterior,
                            @NOVOTIPO as TipoComprovacaoNova,
                            @MOTIVO as Motivo,
                            @DATAALTERACAO as DataAlteracao,
                            @USUARIOALTERACAOID as UsuarioAlteracaoId,
                            OrganizacaoId,
                            null as InativoDesde,
                            0 as Inativo
                           from servidor s {where} ";

                    _repositorio.ConsultaDapper<int>(cmdHistorico, new
                    {
                        @ORGANIZACAOID = sessao.OrganizacaoId,
                        @DEPARTAMENTOID = sessao.DepartamentoId,
                        @UNIDADES = sessao.UnidadeOrganizacionais,
                        @NOVOTIPO = novoTipo,
                        @MOTIVO = motivo.Trim(),
                        @DATAALTERACAO = DateTime.Now,
                        @USUARIOALTERACAOID = sessao.UsuarioId
                    });

                    var cmdUpdateServidor = $@"update servidor s set TipoComprovacaoPontoApp = @NOVOTIPO {where}";
                    _repositorio.ConsultaDapper<int>(cmdUpdateServidor, new
                    {
                        @ORGANIZACAOID = sessao.OrganizacaoId,
                        @DEPARTAMENTOID = sessao.DepartamentoId,
                        @UNIDADES = sessao.UnidadeOrganizacionais,
                        @NOVOTIPO = novoTipo
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}