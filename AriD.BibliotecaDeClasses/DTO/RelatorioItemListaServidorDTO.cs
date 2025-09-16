namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioItemListaServidorDTO
    {
        public int ServidorId { get; set; }
        public string PessoaNome { get; set; }
        public string? PessoaCpf { get; set; }
        public DateTime DataDeNascimento { get; set; }
    }
}