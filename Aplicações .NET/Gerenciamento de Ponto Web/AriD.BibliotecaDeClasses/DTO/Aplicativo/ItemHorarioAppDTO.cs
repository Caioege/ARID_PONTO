using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO.Aplicativo
{
    public class ItemHorarioAppDTO
    {
        public string DiaDaSemana { get; set; }

        public TimeSpan? Entrada1 { get; set; }
        public TimeSpan? Saida1 { get; set; }

        public TimeSpan? Entrada2 { get; set; }
        public TimeSpan? Saida2 { get; set; }

        public TimeSpan? Entrada3 { get; set; }
        public TimeSpan? Saida3 { get; set; }

        public bool Utiliza5Periodos { get; set; }

        public TimeSpan? Entrada4 { get; set; }
        public TimeSpan? Saida4 { get; set; }

        public TimeSpan? Entrada5 { get; set; }
        public TimeSpan? Saida5 { get; set; }

        public TimeSpan? CargaHoraria { get; set; }
    }
}