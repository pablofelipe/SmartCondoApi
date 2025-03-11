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

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ServiceType> ServiceTypes { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Condominium> Condominiums { get; set; }

        public DbSet<Tower> Towers { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Message> Messages{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Login)
                .WithOne(l => l.User)
                .HasForeignKey<Login>(l => l.UserId);

            modelBuilder.Entity<User>()
                .HasIndex(c => c.PersonalTaxID)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(e => e.Condominium)
                .WithMany(e => e.Users)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasOne(e => e.Tower)
                .WithMany(e => e.Users)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Vehicles)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.SentMessages)
                .WithOne(m => m.SenderUser)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.RecipientUser)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata


            modelBuilder.Entity<Login>()
                .HasIndex(c => c.Email)
                .IsUnique();


            modelBuilder.Entity<Condominium>()
                .HasMany(c => c.Users)
                .WithOne(u => u.Condominium)
                .HasForeignKey(u => u.CondominiumId);

            modelBuilder.Entity<Condominium>()
                .HasMany(c => c.Services)
                .WithOne(s => s.Condominium)
                .HasForeignKey(s => s.CondominiumId);


            modelBuilder.Entity<ServiceType>()
                .HasMany(st => st.Services)
                .WithOne(s => s.ServiceType)
                .HasForeignKey(s => s.ServiceTypeId);


            modelBuilder.Entity<Vehicle>()
                .Property(u => u.VehicleId)
                .ValueGeneratedOnAdd(); // Valor gerado automaticamente ao adicionar

            modelBuilder.Entity<Vehicle>()
                .HasIndex(c => c.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasOne(u => u.User)
                .WithMany(v => v.Vehicles)
                .HasForeignKey(f => f.UserId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
