using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoVeiculo
    {
        [Description("Automóvel (Carro)")]
        [Display(Name = "Automóvel (Carro)")]
        Automovel = 0,

        [Description("Motocicleta")]
        [Display(Name = "Motocicleta")]
        Motocicleta = 1,

        [Description("Caminhão")]
        [Display(Name = "Caminhão")]
        Caminhao = 2,

        [Description("Ônibus")]
        [Display(Name = "Ônibus")]
        Onibus = 3,

        [Description("Ambulância")]
        [Display(Name = "Ambulância")]
        Ambulancia = 4
    }
}
