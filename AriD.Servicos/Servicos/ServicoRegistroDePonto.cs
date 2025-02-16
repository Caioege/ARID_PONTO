using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

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
                        EquipamentoDePontoId = dadosEquipamento.Value.Value
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}