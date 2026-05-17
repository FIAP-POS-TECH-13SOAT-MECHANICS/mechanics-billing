using Mechanics.Domain.Base;
using Mechanics.Domain.Payments;
using Mechanics.Infra.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Mechanics.Infra.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Mechanics");
        ConfigurePayment(modelBuilder);
        ConfigureAbstractEntities(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new NormalizationInterceptor());
        base.OnConfiguring(optionsBuilder);
    }

    private static void ConfigurePayment(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Payment>();

        builder.ToTable("Payments");
        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.Status).IsRequired();
        builder.Property(payment => payment.StatusDetail).HasMaxLength(100);
        builder.Property(payment => payment.MercadoPagoPaymentId).HasMaxLength(100);
        builder.Property(payment => payment.MercadoPagoPreferenceId).HasMaxLength(100);
        builder.Property(payment => payment.ExternalReference).HasMaxLength(100).IsRequired();
        builder.Property(payment => payment.CheckoutUrl).HasMaxLength(2000);
        builder.Property(payment => payment.SandboxCheckoutUrl).HasMaxLength(2000);
        builder.Property(payment => payment.UpdatedAt)
            .HasDefaultValueSql("SYSDATETIME()")
            .ValueGeneratedOnAdd();

        builder.HasIndex(payment => payment.MercadoPagoPaymentId);
        builder.HasIndex(payment => payment.MercadoPagoPreferenceId);
        builder.HasIndex(payment => payment.ExternalReference);
    }

    private static void ConfigureAbstractEntities(ModelBuilder modelBuilder)
    {
        var types = modelBuilder.Model.GetEntityTypes()
            .Where(type => typeof(AbstractEntity).IsAssignableFrom(type.ClrType))
            .Select(type => type.ClrType);

        foreach (var type in types)
        {
            modelBuilder.Entity(type)
                .Property(nameof(AbstractEntity.Id))
                .HasDefaultValueSql("NEWID()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity(type)
                .Property(nameof(AbstractEntity.CreationDate))
                .IsRequired()
                .HasDefaultValueSql("SYSDATETIME()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
