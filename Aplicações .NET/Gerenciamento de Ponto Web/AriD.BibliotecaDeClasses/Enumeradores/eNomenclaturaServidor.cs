using AriD.BibliotecaDeClasses.Atributos;
using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    [Description("Nomenclatura do servidor")]
    public enum eNomenclaturaServidor
    {
        [Description("Servidores")]
        [NomenclaturaSingular("Servidor")]
        [NomenclaturaPlural("Servidores")]
        Servidores,

        [Description("Funcionários")]
        [NomenclaturaSingular("Funcionário")]
        [NomenclaturaPlural("Funcionários")]
        Funcionarios,

        [Description("Empregados")]
        [NomenclaturaSingular("Empregado")]
        [NomenclaturaPlural("Empregados")]
        Empregados,

        [Description("Profissionais")]
        [NomenclaturaSingular("Profissional")]
        [NomenclaturaPlural("Profissionais")]
        Profissionais,

        [Description("Colaboradores")]
        [NomenclaturaSingular("Colaborador")]
        [NomenclaturaPlural("Colaboradores")]
        Colaboradores,
    }
}