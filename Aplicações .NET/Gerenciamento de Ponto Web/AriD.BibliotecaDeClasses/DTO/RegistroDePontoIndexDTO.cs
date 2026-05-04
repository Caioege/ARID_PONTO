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
        public int UnidadeOrganizacionalId { get; set; }
        public string UnidadeOrganizacionalNome { get; set; }
        public string Origem { get; set; }
    }
}