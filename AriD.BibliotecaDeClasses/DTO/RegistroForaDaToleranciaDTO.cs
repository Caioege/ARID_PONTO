using System;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RegistroForaDaToleranciaDTO
    {
        public int RegistroId { get; set; }
        public int PontoDoDiaId { get; set; }
        public int VinculoDeTrabalhoId { get; set; }
        public string ServidorNome { get; set; }
        public string UnidadeNome { get; set; }
        public string DepartamentoNome { get; set; }
        public DateTime DataHoraRegistro { get; set; }
        public TimeSpan HorarioEsperado { get; set; }
        public string TipoRegistro { get; set; } // Entrada 1, Saída 1...
        public int MinutosForaTolerancia { get; set; }
        
        public bool? AprovadoForaTolerancia { get; set; }
        public string AcaoAprovacao { get; set; }
        public string UsuarioAprovacaoNome { get; set; }
    }
}
