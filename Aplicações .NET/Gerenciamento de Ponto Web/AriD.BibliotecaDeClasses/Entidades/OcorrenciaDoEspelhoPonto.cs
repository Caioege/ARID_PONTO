using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("OcorrenciaDoEspelhoPonto")]
    public class OcorrenciaDoEspelhoPonto : EntidadeOrganizacaoBase
    {
        public int VinculoDeTrabalhoId { get; set; }
        public string MesReferencia { get; set; } // Formato MM/yyyy
        public string Descricao { get; set; }
        public DateTime DataHoraCadastro { get; set; }
        public int UsuarioCadastroId { get; set; }
        public string UsuarioCadastroNome { get; set; }

        public OcorrenciaDoEspelhoPonto() { }

        public OcorrenciaDoEspelhoPonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            string mesReferencia,
            string descricao,
            int usuarioCadastroId,
            string usuarioCadastroNome)
        {
            OrganizacaoId = organizacaoId;
            VinculoDeTrabalhoId = vinculoDeTrabalhoId;
            MesReferencia = mesReferencia;
            Descricao = descricao;
            UsuarioCadastroId = usuarioCadastroId;
            UsuarioCadastroNome = usuarioCadastroNome;
            DataHoraCadastro = DateTime.Now;
        }

        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }
        public virtual Usuario UsuarioCadastro { get; set; }
    }
}
