using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Configuration;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Models.DTOs.Requests;

namespace TodoApp.AuthenticationUtils
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApiDbContext _apiDbContext;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtAuthenticationService(IOptionsMonitor<JwtConfig> optionsMonitor, UserManager<IdentityUser> userManager, ApiDbContext apiDbContext, TokenValidationParameters tokenValidationParameters)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _userManager = userManager;
            _apiDbContext = apiDbContext;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
            byte[] secret = Encoding.UTF8.GetBytes(_jwtConfig.Secret);
            var key = new SymmetricSecurityKey(secret);
            var claims = GetClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = jwtTokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsReworked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _apiDbContext.RefreshTokens.AddAsync(refreshToken);
            await _apiDbContext.SaveChangesAsync();

            var authResult = new AuthResult()
            {
                Token = jwtToken,
                IsSuccess = true,
                RefreshToken = refreshToken.Token
            };

            return authResult;
        }

        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
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
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
        }

        public async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);
                
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCulture);
                    if (result == false) return null;
                }

                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                DateTime expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.Now) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token has not yet expired"
                    }
                };

                var storedToken = await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.Token);
                if (storedToken == null) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token does not exist"
                    }
                };

                if (storedToken.IsUsed) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token has been used"
                    }
                };

                if (storedToken.IsReworked) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token has been reworked"
                    }
                };

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti) return new AuthResult()
                {
                    IsSuccess = false,
                    Errors = new List<string>()
                    {
                        "Token doesn't match"
                    }
                };

                storedToken.IsUsed = true;
                _apiDbContext.RefreshTokens.Update(storedToken);
                await _apiDbContext.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTimeVal;
        }
    }
}
