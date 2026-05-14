using Mechanics.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
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

        builder.HasOne(payment => payment.WorkOrder)
            .WithMany()
            .HasForeignKey(payment => payment.WorkOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(payment => payment.Budget)
            .WithMany()
            .HasForeignKey(payment => payment.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
