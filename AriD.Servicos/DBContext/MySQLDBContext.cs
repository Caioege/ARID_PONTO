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
                optionsBuilder.UseMySql(connStringProd, ServerVersion.AutoDetect(connStringProd))   
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();

                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

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
        public DbSet<GrupoDePermissao> GrupoDePermissao { get; set; }
        public DbSet<ItemDoGrupoDePermissao> ItemDoGrupoDePermissao { get; set; }
        public DbSet<RegistroAplicativo> RegistroAplicativo { get; set; }
    }
}