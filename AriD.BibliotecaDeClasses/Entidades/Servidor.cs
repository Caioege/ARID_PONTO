using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Servidor : EntidadeOrganizacaoBase
    {
        public Servidor()
        {
            VinculosDeTrabalho = new();
            ListaDeAnexos = new();
            ListaDeObservacoes = new();
        }

        [Required]
        public int PessoaId { get; set; }

        [ForeignKey(nameof(PessoaId))]
        public virtual Pessoa Pessoa { get; set; }

        [Required]
        public DateTime DataDeCadastro { get; set; }

        public virtual List<VinculoDeTrabalho> VinculosDeTrabalho { get; set; }
        public virtual List<AnexoServidor> ListaDeAnexos { get; set; }
        public virtual List<ObservacaoServidor> ListaDeObservacoes { get; set; }

        public bool AcessoAoAplicativo { get; set; }
        public bool RegistroDePontoNoAplicativo { get; set; }
        public bool RegistroManualNoAplicativo { get; set; }
        public bool RegistroDeAtestadoNoAplicativo { get; set; }

        [MaxLength(15)]
        public string? TelefoneDeContato { get; set; }

        [MaxLength(120)]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? SenhaPersonalizadaDeAcesso { get; set; }

        [MaxLength(20)]
        public string? CodigoCRM { get; set; }
        [MaxLength(200)]
        public string? EspecialidadeMedica { get; set; }

        [Required]
        public bool HabilitaExportacaoParaFolhaDePagamento { get; set; }

        [MaxLength(1000)]
        public string? AlertaManutencaoDePonto { get; set; }

        public string Nome => Pessoa?.Nome;
    }
}