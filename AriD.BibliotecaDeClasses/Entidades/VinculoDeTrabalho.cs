using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class VinculoDeTrabalho : EntidadeOrganizacaoBase
    {
        public int ServidorId { get; set; }
        [ForeignKey(nameof(ServidorId))]
        public virtual Servidor Servidor { get; set; }

        public int TipoDoVinculoDeTrabalhoId { get; set; }

        [ForeignKey(nameof(TipoDoVinculoDeTrabalhoId))]
        public virtual TipoDoVinculoDeTrabalho TipoDoVinculoDeTrabalho { get; set; }

        [Required, MaxLength(10)]
        public string Matricula { get; set; }

        public int HorarioDeTrabalhoId { get; set; }
        [ForeignKey(nameof(HorarioDeTrabalhoId))]
        public virtual HorarioDeTrabalho HorarioDeTrabalho { get; set; }

        public int FuncaoId { get; set; }
        [ForeignKey(nameof(FuncaoId))]
        public virtual Funcao Funcao { get; set; }

        public int DepartamentoId { get; set; }
        [ForeignKey(nameof(DepartamentoId))]
        public virtual Departamento Departamento { get; set; }

        [Required]
        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }

        public eSituacaoVinculoDeTrabalho Situacao { get; set; }

        public virtual List<LotacaoUnidadeOrganizacional> Lotacoes { get; set; } = new();

        public virtual List<Afastamento> Afastamentos { get; set; } = new();

        public override string ToString() => $"{Matricula} - {TipoDoVinculoDeTrabalho?.Descricao} [{Situacao.ToString()}]";
    }
}