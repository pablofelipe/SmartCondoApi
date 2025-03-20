using Microsoft.AspNetCore.Identity;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public class UserDependencies(SmartCondoContext context, UserManager<User> userManager, IConfiguration configuration) : IUserDependencies
    {
        public SmartCondoContext Context { get; } = context;
        public UserManager<User> UserManager { get; } = userManager;
        public IConfiguration Configuration { get; } = configuration;
    }
}
