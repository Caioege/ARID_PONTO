namespace AriD.BibliotecaDeClasses.DTO
{
    public class MonitoramentoConectividadeDTO
    {
        public string NumeroSerie { get; set; }
        public DateTime DataHoraUltimoRegistro { get; set; }
    }

    public class EquipamentoConectividadeInfo
    {
        public string Descricao { get; set; }
        public string NumeroSerie { get; set; }
        public DateTime DataHoraUltimoRegistro { get; set; }
        public string UnidadeNome { get; set; }
    }
}
