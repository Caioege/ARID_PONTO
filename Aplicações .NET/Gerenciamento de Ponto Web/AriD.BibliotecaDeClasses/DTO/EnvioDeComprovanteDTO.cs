namespace AriD.BibliotecaDeClasses.DTO
{
    public class EnvioDeComprovanteDTO
    {
        public int OrganizacaoId { get; set; }
        public string OrganizacaoNome { get; set; }
        public bool EnvioDeMensagemWhatsAppExperimental { get; set; }
        public int ServidorId { get; set; }
        public string ServidorNome { get; set; }
        public string ServidorCpf { get; set; }
        public string TelefoneDeContato { get; set; }
    }
}