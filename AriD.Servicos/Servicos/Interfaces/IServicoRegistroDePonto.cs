using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoRegistroDePonto : IServico<RegistroDePonto>
    {
        Task ReceberRegistroDeEquipamento(RegistroEquipamentoDTO dados);
    }
}