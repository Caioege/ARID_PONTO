using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioConferenciaDePontoDTO
    {
        public int ServidorId { get; set; }
        public string ServidorNome { get; set; }
        public string ServidorCpf { get; set; }
        public DateTime DataHora { get; set; }
        public string Origem { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool ForaDaCerca { get; set; }
        public eSituacaoRegistroAplicativo Situacao { get; set; }
    }
}