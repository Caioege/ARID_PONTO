using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioAfastamentODTO
    {
        public string PessoaNome { get; set; }
        public string PessoaCpf { get; set; }
        public string MatriculaContrato { get; set; }
        public string TipoContrato { get; set; }
        public eSituacaoVinculoDeTrabalho SituacaoContrato { get; set; }
        public DateTime InicioAfastamento { get; set; }
        public DateTime? FimAfastamento { get; set; }
        public string JustificativaAusencia { get; set; }
    }
}
