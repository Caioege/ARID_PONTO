using AriD.BibliotecaDeClasses.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AriD.Servicos.DBContext
{
    public partial class MySQLDBContext : Microsoft.EntityFrameworkCore.DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connStringProd = "Server=localhost;User Id=aridponto;Password=aridponto@123;Database=arid_ponto";
                var connString = "Server=localhost;User Id=root;Password=masterkey;Database=arid_ponto";
                optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString))  
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();

                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<RecadoSistema>()
                .HasMany(a => a.ListaDeUsuariosQueLeram)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "Usuario_RecadoSistema",
                    j => j.HasOne<Usuario>()
                          .WithMany()
                          .HasForeignKey("UsuarioId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<RecadoSistema>()
                          .WithMany()
                          .HasForeignKey("RecadoSistemaId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j => {
                        j.HasKey("UsuarioId", "RecadoSistemaId");
                        j.ToTable("Usuario_RecadoSistema");
                    });
        }

        public DbSet<Endereco> Endereco { get; set; }
        public DbSet<Organizacao> Organizacao { get; set; }
        public DbSet<UnidadeOrganizacional> UnidadeOrganizacional { get; set; }
        public DbSet<Departamento> Departamento { get; set; }
        public DbSet<Funcao> Funcao { get; set; }
        public DbSet<HorarioDeTrabalho> HorarioDeTrabalho { get; set; }
        public DbSet<HorarioDeTrabalhoDia> HorarioDeTrabalhoDia { get; set; }
        public DbSet<Pessoa> Pessoa { get; set; }
        public DbSet<Servidor> Servidor { get; set; }
        public DbSet<PontoDoDia> PontoDoDia { get; set; }
        public DbSet<JustificativaDeAusencia> JustificativaDeAusencia { get; set; }
        public DbSet<Afastamento> Afastamento { get; set; }
        public DbSet<EventoAnual> EventoAnual { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Escala> Escala { get; set; }
        public DbSet<CicloDaEscala> CicloDaEscala { get; set; }
        public DbSet<EscalaDoServidor> EscalaDoServidor { get; set; }
        public DbSet<EquipamentoDePonto> EquipamentoDePonto { get; set; }
        public DbSet<RegistroDePonto> RegistroDePonto { get; set; }
        public DbSet<OcorrenciaDoEspelhoPonto> OcorrenciaDoEspelhoPonto { get; set; }
        public DbSet<GrupoDePermissao> GrupoDePermissao { get; set; }
        public DbSet<ItemDoGrupoDePermissao> ItemDoGrupoDePermissao { get; set; }
        public DbSet<RegistroAplicativo> RegistroAplicativo { get; set; }
        public DbSet<AnexoServidor> AnexoServidor { get; set; }
        public DbSet<MotivoDeDemissao> MotivoDeDemissao { get; set; }
        public DbSet<ObservacaoServidor> ObservacaoServidor { get; set; }
        public DbSet<RecadoSistema> RecadoSistema { get; set; }
        public DbSet<RegraHoraExtra> RegraHoraExtra { get; set; }
        public DbSet<FaixaHoraExtra> FaixaHoraExtra { get; set; }
        public DbSet<PontoDoDiaHoraExtra> PontoDoDiaHoraExtra { get; set; }
        public DbSet<LogAuditoriaPonto> LogAuditoriaPonto { get; set; }
        public DbSet<LayoutExportacaoFolhaPagamento> LayoutExportacaoFolhaPagamento { get; set; }
        public DbSet<LayoutExportacaoFolhaPagamentoCampo> LayoutExportacaoFolhaPagamentoCampo { get; set; }
        public DbSet<MapeamentoEventoFolhaPagamento> MapeamentoEventoFolhaPagamento { get; set; }
        public DbSet<FiltroRelatorio> FiltroRelatorio { get; set; }
        public DbSet<LogAuditoriaEscala> LogAuditoriaEscala { get; set; }
        public DbSet<ConfiguracaoBonus> ConfiguracaoBonus { get; set; }
        public DbSet<ConfiguracaoBonusFuncao> ConfiguracaoBonusFuncao { get; set; }
        public DbSet<BonusCalculado> BonusCalculado { get; set; }
        public DbSet<Rota> Rota { get; set; }
        public DbSet<RotaOcorrenciaDesvio> RotaOcorrenciaDesvio { get; set; }
        public DbSet<ParadaRota> ParadaRota { get; set; }
        public DbSet<LocalizacaoRota> LocalizacaoRota { get; set; }
        public DbSet<RotaExecucao> RotaExecucao { get; set; }
        public DbSet<RotaExecucaoEvento> RotaExecucaoEvento { get; set; }
        public DbSet<RotaExecucaoPausa> RotaExecucaoPausa { get; set; }
        public DbSet<RotaExecucaoDesvio> RotaExecucaoDesvio { get; set; }
        public DbSet<RotaExecucaoSincronizacaoOffline> RotaExecucaoSincronizacaoOffline { get; set; }
        public DbSet<Motorista> Motorista { get; set; }
        public DbSet<Veiculo> Veiculo { get; set; }
        public DbSet<MotoristaHistoricoSituacao> MotoristaHistoricoSituacao { get; set; }
        public DbSet<ChecklistItem> ChecklistItem { get; set; }
        public DbSet<ChecklistExecucao> ChecklistExecucao { get; set; }
        public DbSet<ChecklistExecucaoItem> ChecklistExecucaoItem { get; set; }
        public DbSet<Paciente> Paciente { get; set; }
        public DbSet<ManutencaoVeiculo> ManutencaoVeiculo { get; set; }
        public DbSet<RotaPaciente> RotaPaciente { get; set; }
        public DbSet<RotaProfissional> RotaProfissional { get; set; }
    }
}
