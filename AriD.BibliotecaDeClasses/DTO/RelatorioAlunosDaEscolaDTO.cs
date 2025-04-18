using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioAlunosDaEscolaDTO
    {
        public int EscolaId { get; set; }
        public string EscolaNome { get; set; }

        public string PessoaNome { get; set; }
        public string IdEquipamento { get; set; }
        public string Turma { get; set; }
        public eTurno? TurmaTurno { get; set; }
    }
}
