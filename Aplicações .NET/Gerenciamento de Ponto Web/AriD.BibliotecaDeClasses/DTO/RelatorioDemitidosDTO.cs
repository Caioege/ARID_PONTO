using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioDemitidosDTO
    {
        public string PessoaNome { get; set; }
        public string PessoaCpf { get; set; }
        public string MatriculaContrato { get; set; }
        public string TipoContrato { get; set; }
        public eSituacaoVinculoDeTrabalho SituacaoContrato { get; set; }
        public int MotivoDeDemissaoId { get; set; }
        public string MotivoDeDemissaoDescricao { get; set; }
        public string Observacoes { get; set; }
        public DateTime DataDaDemissao { get; set; }
    }
}