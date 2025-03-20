using Microsoft.AspNetCore.Identity;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public interface IUserDependencies
    {
        SmartCondoContext Context { get; }
        IConfiguration Configuration { get; }
        UserManager<User> UserManager { get; }
    }
}
