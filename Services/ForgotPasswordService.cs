using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Dto;
using SmartCondoApi.Models;
using System.Net.Mail;
using System.Security.Cryptography;


namespace SmartCondoApi.Services
{
    public interface IForgotPasswordService
    {
        Task<ForgotPasswordResponseDto> SendResetLinkAsync(ForgotPasswordRequestDto request);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
    public class ForgotPasswordService(SmartCondoContext _context, IConfiguration _configuration) : IForgotPasswordService
    {
        public async Task<ForgotPasswordResponseDto> SendResetLinkAsync(ForgotPasswordRequestDto request)
        {
            var login = await _context.Logins.FirstOrDefaultAsync(l => l.Email == request.Email);
            if (login == null)
            {
                throw new ArgumentException("Usuário não encontrado.");
            }

            // Gera um token único
            var token = GenerateToken();

            // Armazena o token e a data de expiração na nova tabela
            var passwordResetToken = new PasswordResetToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddHours(1), // 1 hora de expiração
                LoginId = login.LoginId
            };

            _context.PasswordResetTokens.Add(passwordResetToken);
            await _context.SaveChangesAsync();

            // Envia o e-mail com o link de reset
            var resetLink = $"http://seusite.com/reset-password/{token}";
            await SendEmailAsync(login.Email, "Redefinição de Senha", $"Clique no link para redefinir sua senha: {resetLink}");

            return new ForgotPasswordResponseDto { Message = "E-mail de redefinição enviado com sucesso." };
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var passwordResetToken = await _context.PasswordResetTokens
                .Include(prt => prt.Login) // Carrega o Login relacionado
                .FirstOrDefaultAsync(prt =>
                    prt.Token == request.Token &&
                    prt.Expires > DateTime.UtcNow);

            if (passwordResetToken == null)
            {
                throw new ArgumentException("Token inválido ou expirado.");
            }

            // Atualiza a senha do Login
            passwordResetToken.Login.Password = HashPassword(request.Password);

            // Remove o token de reset (já foi usado)
            _context.PasswordResetTokens.Remove(passwordResetToken);
            await _context.SaveChangesAsync();

            return new ResetPasswordResponseDto { Message = "Senha redefinida com sucesso." };
        }

        private static string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();

            var tokenBytes = new byte[32];

            rng.GetBytes(tokenBytes);

            // Converte os bytes para uma string hexadecimal
            return Convert.ToHexStringLower(tokenBytes);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var fromEmail = emailSettings["FromEmail"];
            var fromPassword = emailSettings["FromPassword"];

            using var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]))
            {
                Credentials = new System.Net.NetworkCredential(fromEmail, fromPassword),
                EnableSsl = bool.Parse(emailSettings["EnableSsl"]),
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                To = { to },
                Subject = subject,
                Body = body,
            };

            await client.SendMailAsync(mailMessage);
        }

        private string HashPassword(string password)
        {
            return password;
        }
    }
}
