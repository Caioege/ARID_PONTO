namespace AriD.BibliotecaDeClasses.DTO.Aplicativo
{
    public class AutenticacaoAppDTO
    {
        public int ServidorId { get; set; }
        public string ServidorNome { get; set; }

        public int OrganizacaoId { get; set; }
        public string OrganizacaoNome { get; set; }

        public string Cpf { get; set; }

        public string FotoBase64 { get; set; }

        public bool RegistroDePontoNoAplicativo { get; set; }
        public bool RegistroManualNoAplicativo { get; set; }
        public bool RegistroDeAtestadoNoAplicativo { get; set; }
    }
}