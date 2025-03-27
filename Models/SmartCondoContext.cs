using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Dto;
using SmartCondoApi.Models.Permissions;
using System.Security.Claims;

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

        public DbSet<Message> Messages { get; set; }

        public DbSet<UserMessage> UserMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithOne(l => l.UserProfile)
                .HasForeignKey<User>(l => l.Id);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(c => c.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasOne(u => u.UserType)
                .WithMany()
                .HasForeignKey(u => u.UserTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

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

            // Configuração do relacionamento Sender (Remetente)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)          // Cada Message tem um Sender
                .WithMany(u => u.SentMessages)  // Cada UserProfile tem muitas SentMessages
                .HasForeignKey(m => m.SenderId) // Chave estrangeira em Message
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMessage>()
                .HasKey(um => um.Id);

            modelBuilder.Entity<UserMessage>()
                .HasOne(um => um.Message)
                .WithMany(m => m.UserMessages)
                .HasForeignKey(um => um.MessageId);

            modelBuilder.Entity<UserMessage>()
                .HasOne(um => um.UserProfile)
                .WithMany()
                .HasForeignKey(um => um.UserProfileId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

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

        public static async Task SeedPermissionsAsync(RoleManager<IdentityRole<long>> roleManager)
        {
            var rolePermissions = RolePermissions.GetPermissions();

            foreach (var rolePermission in rolePermissions)
            {
                await CreateOrUpdateRoleAsync(roleManager, rolePermission.Key, rolePermission.Value);
            }
        }

        private static async Task CreateOrUpdateRoleAsync(RoleManager<IdentityRole<long>> roleManager,
            string roleName, UserPermissionsDTO permission)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole<long>(roleName);
                await roleManager.CreateAsync(role);
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in existingClaims)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            foreach (var property in permission.GetType().GetProperties())
            {
                var value = property.GetValue(permission);

                if (value != null && bool.TryParse(value.ToString(), out bool bValue) && bValue)
                    await roleManager.AddClaimAsync(role, new Claim("Permission", property.Name));
            }

            foreach (var allowedType in permission.AllowedRecipientTypes)
            {
                await roleManager.AddClaimAsync(role, new Claim("AllowedRecipient", allowedType));
            }
        }
    }
}
