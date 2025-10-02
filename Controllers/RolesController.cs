using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [Authorize(Policy = "SuperAdminPolicy")] // Only SuperAdmin can manage roles
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Role name cannot be empty");

            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest("Role already exists");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (result.Succeeded)
            {
                return Ok(new { message = "Role created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            // Protect system roles
            if (roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("Cashier", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Cannot delete system roles");
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound("Role not found");

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Ok(new { message = "Role deleted successfully" });
            }

            return BadRequest(result.Errors);
        }
    }
}
