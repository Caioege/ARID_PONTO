using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoCombustivel
    {
        Gasolina,
        Etanol,
        Diesel,
        Flex,
        GNV,
        [Description("Elétrico")]
        Eletrico
    }
}