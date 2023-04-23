using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Configuration;
using TodoApp.Models.DTOs.Requests;

namespace TodoApp.AuthenticationUtils
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<IdentityUser> _userManager;

        public JwtAuthenticationService(IOptionsMonitor<JwtConfig> optionsMonitor, UserManager<IdentityUser> userManager)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _userManager = userManager;
        }

        public string GenerateJwtToken(IdentityUser user)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] secret = Encoding.UTF8.GetBytes(_jwtConfig.Secret);
            var key = new SymmetricSecurityKey(secret);
            var claims = GetClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> IsAuthenticated(IdentityUser existingUser, UserLoginRequest login)
        {
            bool isAuthenticated = await _userManager.CheckPasswordAsync(existingUser, login.Password);
            return isAuthenticated;
        }

        private static List<Claim> GetClaims(IdentityUser user)
        {
            return new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
        }

        //private User GetUserModelFromClaim(ClaimsIdentity identity)
        //{
        //    IEnumerable<Claim> userClaims = identity.Claims;

        //    return new User
        //    {
        //        Name = userClaims.ExtractUserClaim(ClaimTypes.Name),
        //        Email = userClaims.ExtractUserClaim(ClaimTypes.Email),
        //        Role = userClaims.ExtractUserClaim(ClaimTypes.Role)
        //    };
        //}
    }
}
