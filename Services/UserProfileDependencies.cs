using Microsoft.AspNetCore.Identity;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public class UserProfileDependencies(SmartCondoContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IEmailService emailService) : IUserProfileDependencies
    {
        public SmartCondoContext Context { get; } = context;
        public UserManager<User> UserManager { get; } = userManager;
        public IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;
        public IEmailService EmailService { get; } = emailService;
    }
}
