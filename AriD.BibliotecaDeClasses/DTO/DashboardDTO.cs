namespace AriD.BibliotecaDeClasses.DTO
{
    public class DashboardDTO
    {
        public int TotalDeAlunosMatriculados { get; set; }
        public int TotalDeRegistrosHoje { get; set; }
        public int TotalDeEquipamentosAtivos { get; set; }

        public Tuple<string[], int[]> RegistrosPorHorario { get; set; } = new Tuple<string[], int[]>(null, null);
        public Tuple<string[], int[]> RegistrosPorEquipamento { get; set; } = new Tuple<string[], int[]>(null, null);

        public List<DashboardRegistroEquipamentoDTO> UltimosRegistrosRecebidos { get; set; } = [];
    }
}