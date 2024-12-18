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
                var connStringProd = "Server=localhost;User Id=arid_wigreja;Password=5bW02_c5x;Database=arid_wigreja";
                var connString = "Server=localhost;User Id=root;Password=masterkey;Database=arid_ponto";
                optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString))
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();

                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

        public DbSet<Organizacao> Organizacao { get; set; }
    }
}