using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PontoEletronico.Models;

namespace PontoEletronico.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TimeRecord> TimeRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuração global para DateTime como UTC
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }

        // Configuração da entidade TimeRecord
        builder.Entity<TimeRecord>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Timestamp)
                .IsRequired();

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relacionamento com ApplicationUser
            entity.HasOne(e => e.User)
                .WithMany(u => u.TimeRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
        });

        // Configuração da entidade ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EmployeeCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Índice único para EmployeeCode
            entity.HasIndex(e => e.EmployeeCode)
                .IsUnique();
        });
    }
}