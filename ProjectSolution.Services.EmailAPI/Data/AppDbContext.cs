using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.EmailAPI.Models;

namespace ProjectSolution.Services.EmailAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<EmailLogger> EmailLoggers { get; set; }
    }
}
