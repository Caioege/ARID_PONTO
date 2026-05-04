using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoComprovacaoPontoApp
    {
        [Description("Nenhuma")]
        Nenhuma = 0,

        [Description("Tirar Selfie")]
        ApenasSelfie = 1,

        [Description("Liveness Facial (Biometria)")]
        LivenessFacial = 2
    }
}
