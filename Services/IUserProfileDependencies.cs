using Microsoft.AspNetCore.Identity;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public interface IUserProfileDependencies
    {
        SmartCondoContext Context { get; }
        IHttpContextAccessor HttpContextAccessor { get; }
        IEmailService EmailService { get; }
        UserManager<User> UserManager { get; }
    }
}
