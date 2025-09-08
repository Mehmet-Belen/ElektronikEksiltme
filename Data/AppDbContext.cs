using Microsoft.EntityFrameworkCore;
using WebApplication1.Models; // User modelini buradan kullanacağız

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<IhaleBilgileri> IhaleBilgileri { get; set; }

    }
}
