using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SMS.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SMS.DBContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<GoldCaratage> Caratages { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure decimal properties for Item entity
            modelBuilder.Entity<Item>()
                .Property(i => i.ItemCaratage)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemGoldWeight)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemValue)
                .HasColumnType("decimal(18, 2)");

            // Configure decimal properties for Transaction entity
            modelBuilder.Entity<Transaction>()
                .Property(t => t.InterestRate)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.SubTotal)
                .HasColumnType("decimal(18, 2)");

            // Configure Customer entity
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CustomerNIC)
                .IsUnique();

            // Configure Item entity
            modelBuilder.Entity<Item>()
                .HasKey(i => i.ItemId);

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemId)
                .ValueGeneratedOnAdd();

            // Configure Invoice entity
            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.InvoiceId);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.InvoiceId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Invoice>()
                .HasIndex(c => c.InvoiceNo)
                .IsUnique();

            // Add other configurations for relationships, indexes, etc., if needed

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
            return $"INVO{nextInvoiceNumber:D3}";
        }
    }
}
