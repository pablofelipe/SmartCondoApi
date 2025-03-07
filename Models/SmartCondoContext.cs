using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        public DbSet<User> Users { get; set; }

        public DbSet<ServiceType> ServiceTypes { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Condominium> Condominiums { get; set; }

        public DbSet<Tower> Towers { get; set; }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Message> Messages{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Login) // User tem um Login
                .WithOne(l => l.User) // Login tem um User
                .HasForeignKey<User>(u => u.LoginId); // Chave estrangeira em User

            modelBuilder.Entity<User>()
                .HasIndex(c => c.PersonalTaxID)
                .IsUnique();

            modelBuilder.Entity<Condominium>()
                .HasMany(c => c.Users)
                .WithOne(u => u.Condominium)
                .HasForeignKey(u => u.CondominiumId);

            modelBuilder.Entity<ServiceType>()
                .HasMany(st => st.Services)
                .WithOne(s => s.ServiceType)
                .HasForeignKey(s => s.ServiceTypeId);

            modelBuilder.Entity<Condominium>()
                .HasMany(c => c.Services)
                .WithOne(s => s.Condominium)
                .HasForeignKey(s => s.CondominiumId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Cars) // Um User tem muitos Cars
                .WithOne(c => c.User) // Um Car pertence a um User
                .HasForeignKey(c => c.UserId); // Chave estrangeira em Car

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.LicensePlate)
                .IsUnique();

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

            base.OnModelCreating(modelBuilder);
        }

        //public override int SaveChanges()
        //{
        //    var entities = ChangeTracker.Entries()
        //        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
        //        .Select(e => e.Entity);

        //    foreach (var entity in entities)
        //    {
        //        var validationContext = new ValidationContext(entity);
        //        Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
        //    }

        //    return base.SaveChanges();
        //}
    }
}
