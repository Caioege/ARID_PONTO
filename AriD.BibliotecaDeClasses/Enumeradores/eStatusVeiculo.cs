using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eStatusVeiculo
    {
        [Description("Disponível")]
        Disponivel,
        [Description("Em Uso")]
        EmUso,
        [Description("Em Manutenção")]
        EmManutencao,
        Vendido,
        Inativo
    }
}