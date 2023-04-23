using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using TodoApp.Models.DTOs.Requests;

namespace TodoApp.AuthenticationUtils
{
    public interface IJwtAuthenticationService
    {
        Task<bool> IsAuthenticated(IdentityUser existingUser, UserLoginRequest login);
        string GenerateJwtToken(IdentityUser user);
        //IdentityUser GetCurrentUserFromHttpContext(HttpContext httpContext);
    }
}
