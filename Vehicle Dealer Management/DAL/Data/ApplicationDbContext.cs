using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Dealer> Dealers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Vehicle entity
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(e => e.VinNumber).IsUnique().HasFilter("[VinNumber] IS NOT NULL");
                entity.HasIndex(e => e.LicensePlate).IsUnique().HasFilter("[LicensePlate] IS NOT NULL");
            });

            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasOne(s => s.Vehicle)
                      .WithMany(v => v.Sales)
                      .HasForeignKey(s => s.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Customer)
                      .WithMany(c => c.Sales)
                      .HasForeignKey(s => s.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

