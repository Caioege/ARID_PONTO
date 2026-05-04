using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoUnidadeOrganizacional
    {
        [Description("Instituição Pública")]
        InstituicaoPublica,

        [Description("Instituição Privada")]
        InstituicaoPrivada,

        [Description("Unidade de Saúde")]
        Saude,

        [Description("Educação")]
        Educacao,

        [Description("Fundação")]
        Fundacao
    }
}