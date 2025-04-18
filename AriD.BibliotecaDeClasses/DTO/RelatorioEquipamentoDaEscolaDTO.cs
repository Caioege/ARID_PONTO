namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioEquipamentoDaEscolaDTO
    {
        public int EscolaId { get; set; }
        public string EscolaNome { get; set; }

        public string EquipamentoDescricao { get; set; }
        public bool EquipamentoAtivo { get; set; }
        public string EquipamentoNumeroDeSerie { get; set; }
    }
}