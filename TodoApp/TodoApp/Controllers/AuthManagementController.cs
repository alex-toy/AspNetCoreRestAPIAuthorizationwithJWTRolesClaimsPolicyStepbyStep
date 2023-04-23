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
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;
        private readonly IJwtAuthenticationService _jwtAuthService;
        private readonly ApiDbContext _apiDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthManagementController> _logger;

        public AuthManagementController(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IJwtAuthenticationService jwtAutService, ApiDbContext apiDbContext, RoleManager<IdentityRole> roleManager, ILogger<AuthManagementController> logger, TokenValidationParameters tokenValidationParameters)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtAuthService = jwtAutService;
            _apiDbContext = apiDbContext;
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

            string token = _jwtAuthService.GenerateJwtToken(newUser);
            return Ok(new RegistrationResponse()
            {
                IsSuccess = true,
                Token = token,
            });
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

            var token = _jwtAuthService.GenerateJwtToken(userDb);
            return Ok(new RegistrationResponse()
            {
                IsSuccess = true,
                Token = token,
            });
        }
    }
}
