namespace AriD.BibliotecaDeClasses.ParametrosDeConsulta
{
    public class ParametrosDeConsultaRegistroDePonto
    {
        public int RedeDeEnsinoId { get; set; }
        public int? EscolaId { get; set; }
        public string Pesquisa { get; set; }
        public int Pagina { get; set; }
        public int TotalPorPagina { get; set; }
    }
}
