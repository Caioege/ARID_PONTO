using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioFrequenciaNaDataDTO
    {
        public int EscolaId { get; set; }
        public string EscolaNome { get; set; }

        public int TurmaId { get; set; }
        public string TurmaDescricao { get; set; }
        public eTurno TurmaTurno { get; set; }

        public string PessoaNome { get; set; }

        public string IdEquipamento { get; set; }

        public bool? PresencaoDiarioDeClasse { get; set; }
        public bool PresencaEquipamento { get; set; }
    }
}