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
    }
}