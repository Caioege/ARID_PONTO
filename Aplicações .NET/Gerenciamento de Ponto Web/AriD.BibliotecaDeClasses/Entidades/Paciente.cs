using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Paciente : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(150)]
        public string Nome { get; set; }

        [MaxLength(14)]
        public string? CPF { get; set; }

        public DateTime? DataNascimento { get; set; }

        [MaxLength(15)]
        public string? Telefone { get; set; }

        [MaxLength(150)]
        public string? AcompanhanteNome { get; set; }

        [MaxLength(14)]
        public string? AcompanhanteCPF { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
