using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            var roleExists = await _roleManager.RoleExistsAsync(name);
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
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }
    }
}
