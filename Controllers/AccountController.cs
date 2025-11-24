using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Serilog;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IPaginationService _paginationService;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IConfiguration configuration,
            IPaginationService paginationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _paginationService = paginationService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                Log.Information("Login attempt for username: {Username}", model.Username);
                
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var token = await GenerateJwtToken(user);
                    var expiration = DateTime.Now.AddHours(1);
                    
                    Log.Information("User {Username} logged in successfully", model.Username);
                    return Ok(new LoginResponse { Token = token, Expiration = expiration });
                }

                Log.Warning("Failed login attempt for username: {Username}", model.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during login for username: {Username}", model.Username);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    Log.Warning("Registration attempt without token for username: {Username}", model.Username);
                    return Unauthorized(new { message = "Token is missing." });
                }

                // Parse the token to get the user ID
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    Log.Warning("Registration attempt with invalid token for username: {Username}", model.Username);
                    return Unauthorized(new { message = "Invalid token." });
                }

                // Extract the user ID from the token claims
                var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    Log.Warning("Registration attempt with token missing user ID for username: {Username}", model.Username);
                    return Unauthorized(new { message = "Token does not contain a valid user ID." });
                }

                var userId = userIdClaim.Value;

                // Get the current user from the database
                var currentUser = await _userManager.FindByIdAsync(userId);

                if (currentUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(currentUser);

                    // Allow SuperAdmin and Admin to register users
                    if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
                    {
                        Log.Warning("Unauthorized registration attempt by user {CurrentUser} for username: {NewUsername}", 
                            currentUser.UserName, model.Username);
                        return Forbid("You are not authorized to register users.");
                    }
                }
                else
                {
                    Log.Warning("Registration attempt with non-existent user ID from token for username: {Username}", model.Username);
                    return Unauthorized(new { message = "You must be logged in as an Admin or SuperAdmin to register new users." });
                }

                // Proceed with user registration
                Log.Information("Creating new user {Username} by {RegisteredBy}", model.Username, currentUser?.UserName);
                
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.Roles.Any())
                    {
                        await _userManager.AddToRoleAsync(user, model.Roles[0]);
                        Log.Information("User {Username} created successfully with role {Role} by {RegisteredBy}", 
                            model.Username, model.Roles[0], currentUser?.UserName);
                    }
                    else
                    {
                        Log.Information("User {Username} created successfully (no role assigned) by {RegisteredBy}", 
                            model.Username, currentUser?.UserName);
                    }
                    return Ok(new { message = "User created successfully" });
                }

                Log.Warning("Failed to create user {Username}. Errors: {Errors}", 
                    model.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(new { message = "Failed to create user", errors = result.Errors });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during user registration for username: {Username}", model.Username);
                return StatusCode(500, new { message = "An error occurred during user registration" });
            }
        }

        [HttpGet("users")]
        [Authorize(Policy = "UserManagementPolicy")] // SuperAdmin and Admin can view users
        public async Task<ActionResult<PaginatedResponse<GetUserDTO>>> GetAllUsers([FromQuery] UserSearchRequest request)
        {
            try
            {
                Log.Information("Fetching users with search: {Search}, role: {Role}, page: {Page}", 
                    request.Search, request.Role, request.Page);
                
                var usersQuery = _userManager.Users.AsQueryable();

                // Apply search
                if (!string.IsNullOrEmpty(request.Search))
                {
                    usersQuery = usersQuery.Where(u => 
                        (u.UserName != null && u.UserName.Contains(request.Search)) ||
                        (u.Email != null && u.Email.Contains(request.Search))
                    );
                }

                var users = usersQuery
                    .OrderBy(u => u.UserName) // Add OrderBy to fix the warning
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var totalUsers = usersQuery.Count();

                var userDTOs = new List<GetUserDTO>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Apply role filter if specified
                    if (!string.IsNullOrEmpty(request.Role) && !roles.Contains(request.Role))
                    {
                        continue;
                    }

                    userDTOs.Add(new GetUserDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        Roles = roles.ToList()
                    });
                }

                var pagination = _paginationService.CreatePaginationMetadata(request.Page, request.PageSize, totalUsers);
                var filters = _paginationService.CreateFilterMetadata(request, new { request.Role });

                var response = new PaginatedResponse<GetUserDTO>
                {
                    Data = userDTOs,
                    Pagination = pagination,
                    Filters = filters
                };

                Log.Information("Retrieved {Count} users (total: {Total})", userDTOs.Count, totalUsers);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching users");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("user/{userId}")]
        [Authorize(Policy = "UserManagementPolicy")] // SuperAdmin and Admin can update users
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDTO userDto)
        {
            try
            {
                Log.Information("Updating user {UserId} to username: {Username}", userId, userDto.Username);
                
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    Log.Warning("Attempted to update non-existent user {UserId}", userId);
                    return NotFound(new { message = "User not found" });
                }

                // Update user's name and email
                user.UserName = userDto.Username;
                user.Email = userDto.Email;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    Log.Warning("Failed to update user {UserId}. Errors: {Errors}", 
                        userId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Failed to update user", errors = updateResult.Errors });
                }

                // Update user's roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToAdd = userDto.Roles.Except(currentRoles);
                var rolesToRemove = currentRoles.Except(userDto.Roles);

                var addRoleResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addRoleResult.Succeeded)
                {
                    Log.Warning("Failed to add roles to user {UserId}. Errors: {Errors}", 
                        userId, string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Failed to add roles", errors = addRoleResult.Errors });
                }

                var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeRoleResult.Succeeded)
                {
                    Log.Warning("Failed to remove roles from user {UserId}. Errors: {Errors}", 
                        userId, string.Join(", ", removeRoleResult.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Failed to remove roles", errors = removeRoleResult.Errors });
                }

                Log.Information("User {UserId} updated successfully. New roles: {Roles}", userId, string.Join(", ", userDto.Roles));
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while updating user" });
            }
        }

        [HttpDelete("users/delete-multiple")]
        [Authorize(Policy = "UserManagementPolicy")] // SuperAdmin and Admin can delete users
        public async Task<IActionResult> DeleteUsers([FromBody] List<string> userIds)
        {
            try
            {
                Log.Information("Attempting to delete {Count} users", userIds.Count);
                
                var deletedCount = 0;
                var deletedUsernames = new List<string>();
                
                foreach (var userId in userIds)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            deletedCount++;
                            deletedUsernames.Add(user.UserName ?? userId);
                            Log.Information("User {Username} (ID: {UserId}) deleted successfully", user.UserName, userId);
                        }
                        else
                        {
                            Log.Warning("Failed to delete user {Username} (ID: {UserId}). Errors: {Errors}", 
                                user.UserName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        Log.Warning("Attempted to delete non-existent user {UserId}", userId);
                    }
                }

                Log.Information("Successfully deleted {DeletedCount} of {TotalCount} users: {Usernames}", 
                    deletedCount, userIds.Count, string.Join(", ", deletedUsernames));
                return Ok(new { message = $"Successfully deleted {deletedCount} users" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting users");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var now = DateTime.Now;
            var expires = now.AddHours(1);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? "")
            };

            var key = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Jwt:Key"] ?? "R2VuZXJpY0RlZmF1bHRTZWNyZXRLZXk="));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Log.Debug("Generated JWT token for user {Username}, expires at {Expiration}", user.UserName, expires);
            
            return tokenString;
        }
    }
}
