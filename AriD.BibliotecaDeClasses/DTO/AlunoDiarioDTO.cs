namespace AriD.BibliotecaDeClasses.DTO
{
    public class AlunoDiarioDTO
    {
        public int AlunoTurmaId { get; set; }
        public string AlunoNome { get; set; }
        public DateTime EntradaNaTurma { get; set; }
        public DateTime SaidaDaTurma { get; set; }

        public Dictionary<DateTime, bool?> Frequencias { get; set; } = [];
    }
}