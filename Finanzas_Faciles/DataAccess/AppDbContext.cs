using Microsoft.EntityFrameworkCore;
using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services;

public class AppDbContext : DbContext
{
    private static string DbPath => Path.Combine(FileSystem.AppDataDirectory, "proyectoreferencia.db3");

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext() : this(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite($"Filename={DbPath}")
        .Options) { }
    public DbSet<GastoFijo> GastosFijos => Set<GastoFijo>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<Retiro> Retiros => Set<Retiro>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite($"Filename={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GastoFijo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Actividad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Ignore(e => e.PrecioVentaSugerido);
            entity.Ignore(e => e.MargenCalculado);
            entity.Ignore(e => e.UtilidadPorUnidad);
        });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreActividad).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Retiro>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Concepto).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EstadoPuntoEquilibrioAlMomento).HasMaxLength(100);
        });
    }
}
