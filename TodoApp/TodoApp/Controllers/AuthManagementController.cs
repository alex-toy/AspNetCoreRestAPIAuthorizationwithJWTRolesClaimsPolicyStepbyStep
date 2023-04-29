using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.AuthenticationUtils;
using TodoApp.Configuration;
using TodoApp.Data;
using TodoApp.Models.DTOs.Requests;
using TodoApp.Models.DTOs.Responses;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IJwtAuthenticationService _jwtAuthService;

        public AuthManagementController(UserManager<IdentityUser> userManager, IJwtAuthenticationService jwtAutService)
        {
            _userManager = userManager;
            _jwtAuthService = jwtAutService;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (!ModelState.IsValid) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "invalid payload" },
                IsSuccess = false
            });

            IdentityUser userDb = await _userManager.FindByEmailAsync(user.Email);
            if (userDb != null) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "User already exists" },
                IsSuccess = false
            });

            IdentityUser newUser = new IdentityUser() { Email = user.Email, UserName = user.Name };
            IdentityResult creation = await _userManager.CreateAsync(newUser, user.Password);
            if (!creation.Succeeded) return BadRequest(new RegistrationResponse()
            {
                Errors = creation.Errors.Select(err => err.Description).ToList(),
                IsSuccess = false
            });

            await _userManager.AddToRoleAsync(newUser, "AppUser");

            AuthResult jwtToken = await _jwtAuthService.GenerateJwtToken(newUser);
            return Ok(jwtToken);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userLogin)
        {
            if (!ModelState.IsValid) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "invalid payload" },
                IsSuccess = false
            });

            IdentityUser userDb = await _userManager.FindByEmailAsync(userLogin.Email);
            if (userDb == null) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "User doesn't exists" },
                IsSuccess = false
            });

            bool isAuthenticated = await _jwtAuthService.IsAuthenticated(userDb, userLogin);

            if (!isAuthenticated) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "User not authenticated" },
                IsSuccess = false
            });

            var jwtToken = await _jwtAuthService.GenerateJwtToken(userDb);
            return Ok(jwtToken);
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "invalid payload" },
                IsSuccess = false
            });

            AuthResult result = await _jwtAuthService.VerifyAndGenerateToken(tokenRequest);
            if (result == null) return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string> { "invalid token" },
                IsSuccess = false
            });

            return Ok(result);
        }
    }
}
