using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SMS.Models;
using SMS.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register/{token}")]
        [AllowAnonymous] // Allow anonymous access for registration
        public async Task<IActionResult> Register([FromBody] UserModel model, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing.");
            }

            // Parse the token to get the user ID
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return Unauthorized("Invalid token.");
            }

            // Extract the user ID from the token claims
            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("Token does not contain a valid user ID.");
            }

            var userId = userIdClaim.Value;

            // Get the current user from the database
            var currentUser = await _userManager.FindByIdAsync(userId);

            if (currentUser != null)
            {
                var roles = await _userManager.GetRolesAsync(currentUser);

                if (!roles.Contains("Admin"))
                {
                    return Forbid("You are not authorized to register users.");
                }
            }
            else
            {
                return Unauthorized("You must be logged in as an Admin to register new users.");
            }

            // Proceed with user registration
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Roles[0]);
                return Ok(new { message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateJwtToken(user);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            string finalToken = new JwtSecurityTokenHandler().WriteToken(token);

            return finalToken;
        }

        [HttpGet("users")]
         // Ensure only authenticated users can access this method
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();

            var userDetailsList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDetailsList.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    Roles = roles
                });
            }

            return Ok(userDetailsList);
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserDTO userDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            // Update user's name and email
            user.UserName = userDto.Username;
            user.Email = userDto.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

            // Update user's roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = new List<string> { userDto.Roles[0] }.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(new List<string> { userDto.Roles[0] });

            var addRoleResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addRoleResult.Succeeded)
                return BadRequest(addRoleResult.Errors);

            var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeRoleResult.Succeeded)
                return BadRequest(removeRoleResult.Errors);

            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("users/delete-multiple")]
        public async Task<IActionResult> DeleteUser([FromBody] List<string> userIds)
        {
            foreach(var userId in userIds)
            {

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    continue;
                }

                var result = await _userManager.DeleteAsync(user);


                if (result.Succeeded)
                {
                    continue;
                }
                else
                {

                    //log
                }

            }

            return Ok(new { message = "Users deleted successfully" });

        }
    }
}

