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

        public void ExecuteAcaoEmLote(int acao, SessaoDTO sessao)
        {
            try
            {
                var campo = string.Empty;
                var valor = acao % 2 == 0 ? "false" : "true";

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
            catch (Exception)
            {
                throw;
            }
        }
    }
}