using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class ItemRelatorioServidorPorEscalaDTO
    {
        public int ServidorId { get; set; }
        public string PessoaNome { get; set; }
        public string PessoaCpf { get; set; }
        public string MatriculaVinculo { get; set; }
        public string TipoContrato { get; set; }
        public int EscalaId { get; set; }
        public string EscalaDescricao { get; set; }
        public eTipoDeEscala EscalaTipo { get; set; }
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; }
    }
}