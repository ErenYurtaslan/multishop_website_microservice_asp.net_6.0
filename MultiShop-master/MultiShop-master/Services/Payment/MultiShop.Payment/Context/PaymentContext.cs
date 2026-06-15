using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Entities;

namespace MultiShop.Payment.Context
{
    public class PaymentContext : DbContext
    {
        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options) { }

        public DbSet<PaymentRecord> PaymentRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PaymentRecord>(b =>
            {
                b.HasKey(x => x.PaymentRecordId);
                b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                b.Property(x => x.CardLast4).HasMaxLength(4);
                b.Property(x => x.CardHolderName).HasMaxLength(120);
                b.Property(x => x.Status).HasMaxLength(40);
                b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.OrderSummary).HasColumnType("nvarchar(max)");
            });
        }
    }
}
