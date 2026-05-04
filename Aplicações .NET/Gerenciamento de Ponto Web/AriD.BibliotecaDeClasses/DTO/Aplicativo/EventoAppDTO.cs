using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO.Aplicativo
{
    public class EventoAppDTO
    {
        public string Descricao { get; set; }
        public DateTime Data { get; set; }

        // 0 = Feriado; 1 = Facultativo;
        public eTipoDeEvento TipoEvento { get; set; }
    }
}