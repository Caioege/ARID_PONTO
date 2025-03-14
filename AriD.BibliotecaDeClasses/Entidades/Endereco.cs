using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Endereco : EntidadeBase
    {
        [MaxLength(8)]
        public string? Cep { get; set; }

        [MaxLength(100)]
        public string? Logradouro { get; set; }

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [MaxLength(10)]
        public string? Numero { get; set; }

        [MaxLength(100)]
        public string? Bairro { get; set; }

        [MaxLength(100)]
        public string? Cidade { get; set; }

        public eEstadosDoBrasil? UF { get; set; }

        public override string ToString()
        {
            var listaDeItens = new List<string>();

            if (!string.IsNullOrEmpty(Cep))
                listaDeItens.Add(Cep);

            if (!string.IsNullOrEmpty(Logradouro))
                listaDeItens.Add(Logradouro);

            if (!string.IsNullOrEmpty(Complemento))
                listaDeItens.Add(Complemento);

            if (!string.IsNullOrEmpty(Bairro))
                listaDeItens.Add(Bairro);

            if (!string.IsNullOrEmpty(Cidade))
                listaDeItens.Add(Cidade);

            if (Id > 0)
                listaDeItens.Add(UF.ToString());

            return string.Join(", ", listaDeItens);
        }
    }
}
