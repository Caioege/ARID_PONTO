namespace AriD.BibliotecaDeClasses.DTO
{
    public class DashboardDTO
    {
        public int TotalDeAlunosMatriculados { get; set; }
        public int TotalDeRegistrosHoje { get; set; }
        public int TotalDeEquipamentosAtivos { get; set; }

        public Tuple<string[], int[]> RegistrosPorHorario { get; set; }

        public List<DashboardRegistroEquipamentoDTO> UltimosRegistrosRecebidos { get; set; } = [];
    }
}