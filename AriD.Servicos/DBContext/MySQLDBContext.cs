using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Extensao;
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
                var connStringProdKingHost = "Server=mysql03-farm36.kinghost.net;User Id=aridponto;Password=aridponto2021;Database=aridponto";
                var connStringProd = "Server=localhost;User Id=aridescolas;Password=aridescolas@123;Database=arid_escolas";
                var connString = "Server=localhost;User Id=root;Password=masterkey;Database=arid_escolas";
                optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString))
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();

                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

        public DbSet<Endereco> Endereco { get; set; }
        public DbSet<RedeDeEnsino> RedeDeEnsino { get; set; }
        public DbSet<Escola> Escola { get; set; }
        public DbSet<Pessoa> Pessoa { get; set; }
        public DbSet<Aluno> Aluno { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<EquipamentoDeFrequencia> EquipamentoDeFrequencia { get; set; }
        public DbSet<RegistroDePonto> RegistroDePonto { get; set; }
        public DbSet<GrupoDePermissao> GrupoDePermissao { get; set; }
        public DbSet<ItemDoGrupoDePermissao> ItemDoGrupoDePermissao { get; set; }
        public DbSet<Turma> Turma { get; set; }
        public DbSet<ItemHorarioDeAula> ItemHorarioDeAula { get; set; }
        public DbSet<FrequenciaAlunoTurma> FrequenciaAlunoTurma { get; set; }
    }
}
