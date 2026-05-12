using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.OrderAPI.Models;
using ProjectSolution.Services.OrderAPI.Models.Dto;

namespace ProjectSolution.Services.OrderAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderHeader>()
                .Property(o => o.OrderTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderHeader>()
                .Property(o => o.Discount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetails>()
                .Property(o => o.ProductPrice)
                .HasPrecision(18, 2);
        }
    }
}
