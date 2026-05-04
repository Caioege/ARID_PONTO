namespace AriD.BibliotecaDeClasses.Configuracoes
{
    public class EmailConfig
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string RemetenteEmail { get; set; }
        public string RemetenteSenha { get; set; }
        public string? SendGridApiKey { get; set; }
        public bool UseSendGrid { get; set; }
    }
}
