using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoRegistroDePonto : IServico<RegistroDePonto>
    {
        Task ReceberRegistroDeEquipamento(RegistroEquipamentoDTO dados);
        (int Total, List<RegistroDePontoIndexDTO> Itens) ObtenhaListaPaginadaDTO(
            ParametrosDeConsultaRegistroDePonto parametros);
    }
}
