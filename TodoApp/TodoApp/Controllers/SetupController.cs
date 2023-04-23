using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Data;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SetupController> _logger;

        public SetupController(ApiDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole(string name)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(name);
            if (roleExists) return BadRequest(new { error = "role already exists" });

            var identityRole = new IdentityRole(name);
            IdentityResult roleResult = await _roleManager.CreateAsync(identityRole);

            if (!roleResult.Succeeded)
            {
                string errorMessage = $"The role {name} has not been added successfully.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            string message = $"The role {name} has been added successfully.";
            _logger.LogInformation(message);
            return Ok(new { result = message });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult> GetAllUsers()
        {
            List<IdentityUser> users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("AddUserToRole")]
        public async Task<ActionResult> AddUserToRole(string email, string roleName)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                string errorMessage = $"The user {email} doesn't exist.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                string errorMessage = $"Role {roleName} doesn't exist.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            IdentityResult result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                string errorMessage = $"The user {user.UserName} has not been added successfully to role {roleName}.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            string message = $"The user {user.UserName} has been added successfully to role {roleName}.";
            _logger.LogInformation(message);
            return Ok(new { result = message });
        }

        [HttpGet]
        [Route("GetUserRole")]
        public async Task<ActionResult> GetUserRole(string email)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                string errorMessage = $"The user {email} doesn't exist.";
                _logger.LogInformation(errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            return Ok(roles);
        }
    }
}
