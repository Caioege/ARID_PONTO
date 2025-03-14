using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class ItemRelatorioServidorPorHorarioDTO
    {
        public int PessoaId { get; set; }
        public string PessoaNome { get; set; }
        public string PessoaCpf { get; set; }
        public string ContratoMatricula { get; set; }
        public eSituacaoVinculoDeTrabalho ContratoSituacao { get; set; }
        public string ContratoTipo { get; set; }
        public string HorarioDeTrabalho { get; set; }
    }
}
