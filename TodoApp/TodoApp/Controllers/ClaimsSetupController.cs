using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoApp.Data;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsSetupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ClaimsSetupController> _logger;

        public ClaimsSetupController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ClaimsSetupController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetAllClaims")]
        public async Task<ActionResult> GetAllClaims(string email)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                string errorMessage = $"The user {email} doesn't exist.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

            return Ok(userClaims);
        }

        [HttpPost]
        [Route("AddClaimToUser")]
        public async Task<ActionResult> AddClaimToUser(string email, string claimName, string claimValue)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                string errorMessage = $"The user {email} doesn't exist.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            Claim userClaim = new Claim(claimName, claimValue);

            IdentityResult result = await _userManager.AddClaimAsync(user, userClaim);

            if (!result.Succeeded) return BadRequest(new { error = $"claim {claimName} could not be added to user {user.Email}" });

            return Ok(new { result = $"claim {claimName} was successfully be added to user {user.Email}" });
        }
    }
}
