using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SMS.DBContext.ApplicationDbContext;

namespace SMS.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // Change from DbContext to IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                    : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }
        public DbSet<Karat> Karats { get; set; }
        public DbSet<Pricing> Pricings { get; set; }

        public DbSet<InvoiceTypes> InvoiceTypes { get; set; }
        public DbSet<Installment> Installments { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanPeriod> LoanPeriods { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring the relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Customer)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Invoice)
                .WithOne(i => i.Transaction)
                .HasForeignKey<Invoice>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionItem>()
                .HasKey(ti => ti.TransactionItemId);

            modelBuilder.Entity<TransactionItem>()
                .HasOne(ti => ti.Transaction)
                .WithMany(t => t.TransactionItems)
                .HasForeignKey(ti => ti.TransactionId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to Restrict

            modelBuilder.Entity<TransactionItem>()
                .HasOne(ti => ti.Item)
                .WithMany(i => i.TransactionItems)
                .HasForeignKey(ti => ti.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Transaction)
                .WithOne(t => t.Invoice)
                .HasForeignKey<Invoice>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to Restrict

            // Configuring decimal properties with precision and scale
            modelBuilder.Entity<Transaction>()
                .Property(t => t.SubTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.InterestRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemCaratage)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemGoldWeight)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemValue)
                .HasColumnType("decimal(18,2)");

            /*

            modelBuilder.Entity<Pricing>()
                .HasOne(p => p.Karat)
                .WithMany(k => k.Pricings)
                .HasForeignKey(p => p.KaratId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pricing>()
                .HasOne(p => p.LoanPeriod)
                .WithMany(lp => lp.Pricings)
                .HasForeignKey(p => p.LoanPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
            */
            modelBuilder.Entity<InvoiceTypes>()
               .Property(b => b.InvoiceTypeNumber)
               .HasColumnName("InvoiceTypeNumber")
               .IsRequired();

            modelBuilder.Entity<InvoiceTypes>()
                .Property(b => b.InvoiceTypeName)
                .HasColumnName("InvoiceTypeName")
                .IsRequired()
                .HasMaxLength(50); // Adjust length as necessary

            // Additional configurations can go here

            // Seed data example
            modelBuilder.Entity<InvoiceTypes>().HasData(
                new InvoiceTypes { InvoiceTypeId = 1, InvoiceTypeNumber = 1, InvoiceTypeName = "Initial Pawn Invoice" },
                new InvoiceTypes { InvoiceTypeId = 2, InvoiceTypeNumber = 2, InvoiceTypeName = "Installment Payment Invoice" },
                new InvoiceTypes { InvoiceTypeId = 3, InvoiceTypeNumber = 3, InvoiceTypeName = "Settlement Invoice" }
            );

            // Configure the relationship between Loan and Installment
            modelBuilder.Entity<Installment>()
                .HasOne(i => i.Loan)
                .WithMany(l => l.Installments)
                .HasForeignKey(i => i.LoanId)
                .OnDelete(DeleteBehavior.Restrict); // This prevents cascading delete

            // Configure the relationship between Installment and Transaction
            modelBuilder.Entity<Installment>()
                .HasOne(i => i.Transaction)
                .WithMany(t => t.Installments)
                .HasForeignKey(i => i.TransactionId)
                .OnDelete(DeleteBehavior.Restrict); // This prevents cascading delete

            // Additional configuration to ensure decimal precision and scale for other fields
            modelBuilder.Entity<Installment>()
                .Property(i => i.AmountPaid)
                .HasColumnType("decimal(18,2)"); // Setting precision and scale

            modelBuilder.Entity<Pricing>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<TransactionItem>()
                .Property(ti => ti.PawnValue)
                .HasColumnType("decimal(18,2)");

            base.OnModelCreating(modelBuilder);

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        public override int SaveChanges()
        {
            GenerateInvoiceNumbers();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            GenerateInvoiceNumbers();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void GenerateInvoiceNumbers()
        {
            var newInvoices = ChangeTracker.Entries<Invoice>()
                .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.InvoiceNo))
                .Select(e => e.Entity);

            foreach (var invoice in newInvoices)
            {
                invoice.InvoiceNo = GenerateInvoiceNumber();
            }
        }

        private string GenerateInvoiceNumber()
        {
            var lastInvoice = Invoices.OrderByDescending(i => i.InvoiceId).FirstOrDefault();

            int nextInvoiceNumber = lastInvoice == null ? 1 : lastInvoice.InvoiceId + 1;

            // Get today's date in the specified format
            string todaysDate = DateTime.Today.ToString("yyyyMMdd");

            return $"GC{todaysDate}{nextInvoiceNumber}";
        }


    }
}
