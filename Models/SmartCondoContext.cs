using Microsoft.EntityFrameworkCore;

namespace SmartCondoApi.Models
{
    public class SmartCondoContext : DbContext
    {
        public SmartCondoContext()
        {
            
        }

        public SmartCondoContext(DbContextOptions<SmartCondoContext> options)
            : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");

        public DbSet<Login> Logins { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<ServiceType> ServiceTypes { get; set; }

        public DbSet<User> Users { get; set; }

    }
}
