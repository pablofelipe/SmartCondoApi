using Microsoft.AspNetCore.Mvc;

namespace SmartCondoApi.Services.Auth
{
    public interface IAuthService
    {
        Task<string> Login([FromBody] Dictionary<string, string> body);
    }
}
