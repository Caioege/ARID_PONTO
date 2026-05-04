using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioServidorPorLotacaoDTO
    {
        public int ServidorId { get; set; }
        public string ServidorNome { get; set; }
        public string ServidorCpf { get; set; }
        public int UnidadeId { get; set; }
        public string UnidadeNome { get; set; }
        public string Logradouro { get; set; }
        public string Cep { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public eEstadosDoBrasil? UF { get; set; }
        public DateTime Entrada { get; set; }
        public string VinculoMatricula { get; set; }
        public string HorarioDeTrabalho { get; set; }
        public string TipoVinculo { get; set; }

        public string EnderecoCompleto 
        { 
            get
            {
                var listaDeItens = new List<string>();

                if (!string.IsNullOrEmpty(Logradouro))
                    listaDeItens.Add(Logradouro);

                if (!string.IsNullOrEmpty(Bairro))
                    listaDeItens.Add(Bairro);

                if (!string.IsNullOrEmpty(Cidade))
                    listaDeItens.Add(Cidade);

                if (UF.HasValue)
                    listaDeItens.Add(UF.Value.ToString());

                return string.Join(", ", listaDeItens);
            }
        }
    }
}