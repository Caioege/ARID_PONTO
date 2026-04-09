using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Rota : EntidadeOrganizacaoBase
    {
        public int MotoristaId { get; set; }
        [ForeignKey(nameof(MotoristaId))]
        public virtual Motorista Motorista { get; set; }

        public virtual ICollection<RotaVeiculo> VeiculosDaRota { get; set; } = new List<RotaVeiculo>();

        public string Descricao { get; set; }
        
        public eStatusRota Situacao { get; set; }
        public bool Recorrente { get; set; }

        public DateTime? DataParaExecucao { get; set; }
        public string? NomePaciente { get; set; }
        public string? MedicoResponsavel { get; set; }
        public string? Observacao { get; set; }
        public string? PolylineOficial { get; set; }

        public int? UnidadeDestinoId { get; set; }
        [ForeignKey(nameof(UnidadeDestinoId))]
        public virtual UnidadeOrganizacional UnidadeDestino { get; set; }

        public virtual ICollection<ParadaRota> Paradas { get; set; } = new List<ParadaRota>();
        public virtual ICollection<RotaExecucao> Execucoes { get; set; } = new List<RotaExecucao>();
        public virtual ICollection<RotaPaciente> ListaDePacientes { get; set; } = new List<RotaPaciente>();
        public virtual ICollection<RotaProfissional> ListaDeProfissionais { get; set; } = new List<RotaProfissional>();
    }
}
