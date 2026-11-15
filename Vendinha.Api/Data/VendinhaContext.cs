using Microsoft.EntityFrameworkCore;
using Vendinha.Api.Models;

namespace Vendinha.Api.Data;

public class VendinhaContext : DbContext
{
    public VendinhaContext(DbContextOptions<VendinhaContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Divida> Dividas => Set<Divida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(cliente =>
        {
            cliente.Property(c => c.NomeCompleto).IsRequired().HasMaxLength(150);
            cliente.Property(c => c.Cpf).IsRequired().HasMaxLength(11);
            cliente.Property(c => c.Email).HasMaxLength(254);
            cliente.HasIndex(c => c.Cpf).IsUnique();
        });

        modelBuilder.Entity<Divida>(divida =>
        {
            divida.HasOne(d => d.Cliente).WithMany(c => c.Dividas)
                .HasForeignKey(d => d.ClienteId).OnDelete(DeleteBehavior.Cascade);
            divida.HasIndex(d => d.ClienteId).IsUnique().HasFilter("\"Paga\" = 0");
            divida.ToTable("Dividas", tabela =>
            {
                tabela.HasCheckConstraint("CK_Dividas_Valor", "\"ValorCentavos\" > 0");
                tabela.HasCheckConstraint("CK_Dividas_Pagamento",
                    "(\"Paga\" = 0 AND \"DataPagamento\" IS NULL) OR " +
                    "(\"Paga\" = 1 AND \"DataPagamento\" IS NOT NULL)");
            });
        });
    }
}
