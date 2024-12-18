using AriD.BibliotecaDeClasses.Entidades.Base;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Pessoa : EntidadeOrganizacaoBase
    {
        public string Nome { get; set; }
        public string NomeSocial { get; set; }

        public string Cpf { get; set; }
        public string Rg { get; set; }

        public DateTime? DataDeNascimento { get; set; }

        public int? EnderecoId { get; set; }
        public virtual Endereco Endereco { get; set; }
    }
}