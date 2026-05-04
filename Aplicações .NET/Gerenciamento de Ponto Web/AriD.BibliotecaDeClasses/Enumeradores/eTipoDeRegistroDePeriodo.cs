using AriD.BibliotecaDeClasses.Atributos;
using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoDeRegistroDePeriodo
    {
        [Description("Sem Registro")]
        SemRegistro,

        [Description("Registro Manual")]
        [DescricaoTipoRegistroDePonto("*")]
        RegistroManual,

        [Description("Registro de Equipamento de Ponto")]
        RegistroEquipamento,

        [Description("Registro do Aplicativo de Ponto")]
        [DescricaoTipoRegistroDePonto("ª")]
        RegistroAplicativo,

        [Description("Automático")]
        [DescricaoTipoRegistroDePonto("^")]
        Automatico,
    }
}