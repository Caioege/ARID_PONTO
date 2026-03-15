namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioAuditoriaDeAusenciasDTO
    {
        public string ServidorNome { get; set; } = string.Empty;
        public string Justificativa { get; set; } = string.Empty;
        public DateTime InicioAfastamento { get; set; }
        public DateTime? FimAfastamento { get; set; }
        public string OperadorNome { get; set; } = string.Empty;
        public DateTime DataHoraAcao { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }
}
