using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using TodoApp.Configuration;
using TodoApp.Models.DTOs.Requests;

namespace TodoApp.AuthenticationUtils
{
    public interface IJwtAuthenticationService
    {
        Task<bool> IsAuthenticated(IdentityUser existingUser, UserLoginRequest login);
        Task<AuthResult> GenerateJwtToken(IdentityUser user);
        Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest);
    }
}
