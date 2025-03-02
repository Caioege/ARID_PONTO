namespace AriD.BibliotecaDeClasses.ParametrosDeConsulta
{
    public class ParametrosDeConsultaRegistroDePonto
    {
        public int OrganizacaoId { get; set; }
        public List<int> Unidades { get; set; } = new List<int>();
        public string Pesquisa { get; set; }
        public int Pagina { get; set; }
        public int TotalPorPagina { get; set; }
    }
}