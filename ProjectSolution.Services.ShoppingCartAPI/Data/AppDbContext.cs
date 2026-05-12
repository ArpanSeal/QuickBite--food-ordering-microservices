using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.ShoppingCartAPI.Models;

namespace ProjectSolution.Services.ShoppingCartAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<CartHeader> CartsHeaders { get; set; }
        public DbSet<CartDetails> CartsDetails { get; set; }
    }
}
