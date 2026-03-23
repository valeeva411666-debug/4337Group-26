using Microsoft.EntityFrameworkCore;

namespace Group4337
{
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
     => options.UseSqlServer(
         "Server=DESKTOP-P3I5TR4;Database=ValeevaOrder;Integrated Security=True;TrustServerCertificate=True;");
    }
}