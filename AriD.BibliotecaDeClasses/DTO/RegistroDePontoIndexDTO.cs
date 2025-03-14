namespace AriD.BibliotecaDeClasses.DTO
{
    public class RegistroDePontoIndexDTO
    {
        public int Id { get; set; }
        public int EquipamentoId { get; set; }
        public string EquipamentoDescricao { get; set; }
        public DateTime DataHoraRegistro { get; set; }
        public DateTime DataHoraRecebimento { get; set; }
        public string IdEquipamento { get; set; }
        public string PessoaNome { get; set; }
        public int EscolaId { get; set; }
        public string EscolaNome { get; set; }
    }
}
