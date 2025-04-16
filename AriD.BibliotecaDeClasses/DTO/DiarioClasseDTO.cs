using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class DiarioClasseDTO
    {
        public List<AlunoDiarioDTO> Alunos { get; set; }
        public List<ItemHorarioDeAula> Horarios { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
    }
}