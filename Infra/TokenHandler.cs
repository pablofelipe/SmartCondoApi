using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartCondoApi.Infra
{
    public sealed class TokenHandler(IConfiguration configuration)
    {
        public string Generate(string sub, string email, string role, DateTime expires)
        {
            return GenerateJwtToken(sub, email, role, expires);
        }

        private string GenerateJwtToken(string sub, string email, string role, DateTime expires)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, sub),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
