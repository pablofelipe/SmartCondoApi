using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SmartCondoApi.Models
{
    public class SmartCondoContext : IdentityDbContext<User, IdentityRole<long>, long>
    {
        public SmartCondoContext()
        {
            
        }

        public SmartCondoContext(DbContextOptions<SmartCondoContext> options)
            : base(options)
        {

        }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<ServiceType> ServiceTypes { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Condominium> Condominiums { get; set; }

        public DbSet<Tower> Towers { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Message> Messages{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithOne(l => l.UserProfile)
                .HasForeignKey<User>(l => l.Id);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(c => c.PersonalTaxID)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasOne(u => u.UserType)
                .WithMany()
                .HasForeignKey(u => u.UserTypeId)
                .IsRequired();

            modelBuilder.Entity<UserProfile>()
                .HasOne(e => e.Condominium)
                .WithMany(e => e.Users)
                .IsRequired(false);

            modelBuilder.Entity<UserProfile>()
                .HasOne(e => e.Tower)
                .WithMany(e => e.Users)
                .IsRequired(false);

            modelBuilder.Entity<UserProfile>()
                .HasMany(u => u.Vehicles)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            modelBuilder.Entity<UserProfile>()
                .HasMany(u => u.SentMessages)
                .WithOne(m => m.SenderUser)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            modelBuilder.Entity<UserProfile>()
                .HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.RecipientUser)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            modelBuilder.Entity<User>()
                .HasIndex(c => c.Email)
                .IsUnique();

            //modelBuilder.Entity<PasswordResetToken>()
            //    .HasOne(prt => prt.User) 
            //    .WithMany(l => l.PasswordResetTokens)
            //    .HasForeignKey(prt => prt.LoginId)
            //    .OnDelete(DeleteBehavior.Cascade);

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
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

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
