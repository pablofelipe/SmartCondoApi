using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public DbSet<Message> Messages{ get; set; }

        public DbSet<UserMessage> UserMessages { get; set; }

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
            // Permissões para SystemAdministrator
            await CreateOrUpdateRoleAsync(roleManager, "SystemAdministrator", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator", "Resident", "Janitor"]
            });

            // Permissões para CondominiumAdministrator
            await CreateOrUpdateRoleAsync(roleManager, "CondominiumAdministrator", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Resident", "Janitor"]
            });

            // Permissões para Resident
            await CreateOrUpdateRoleAsync(roleManager, "Resident", new UserTypePermission
            {
                CanSendToIndividuals = false,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator"]
            });

            // Permissões para Janitor
            await CreateOrUpdateRoleAsync(roleManager, "Janitor", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator", "Resident"]
            });

            // Permissões para Doorman
            await CreateOrUpdateRoleAsync(roleManager, "Doorman", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator", "Resident"]
            });

            // Permissões para Cleaner
            await CreateOrUpdateRoleAsync(roleManager, "Cleaner", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Janitor", "CleaningManager"]
            });

            // Permissões para Security
            await CreateOrUpdateRoleAsync(roleManager, "Security", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = false,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Janitor"]
            });

            // Permissões para ServiceProvider
            await CreateOrUpdateRoleAsync(roleManager, "ServiceProvider", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Resident"]
            });

            // Permissões para ExternalProvider
            await CreateOrUpdateRoleAsync(roleManager, "ExternalProvider", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = false,
                AllowedRecipientTypes = ["Resident"]
            });

            // Permissões para DeliveryPerson
            await CreateOrUpdateRoleAsync(roleManager, "DeliveryPerson", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["Resident"]
            });

            // Permissões para CleaningManager
            await CreateOrUpdateRoleAsync(roleManager, "CleaningManager", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Janitor", "Cleaner"]
            });

            // Permissões para Visitor
            await CreateOrUpdateRoleAsync(roleManager, "Visitor", new UserTypePermission
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = false,
                AllowedRecipientTypes = ["Resident"]
            });
        }

        private static async Task CreateOrUpdateRoleAsync(RoleManager<IdentityRole<long>> roleManager,
            string roleName, UserTypePermission permission)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole<long>(roleName);
                await roleManager.CreateAsync(role);
            }

            // Remove todas as claims existentes primeiro
            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in existingClaims)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            // Adiciona as novas claims
            if (permission.CanSendToIndividuals)
                await roleManager.AddClaimAsync(role, new Claim("Permission", "CanSendToIndividuals"));

            if (permission.CanSendToGroups)
                await roleManager.AddClaimAsync(role, new Claim("Permission", "CanSendToGroups"));

            if (permission.CanReceiveMessages)
                await roleManager.AddClaimAsync(role, new Claim("Permission", "CanReceiveMessages"));

            foreach (var allowedType in permission.AllowedRecipientTypes)
            {
                await roleManager.AddClaimAsync(role, new Claim("AllowedRecipient", allowedType));
            }
        }
    }
}
