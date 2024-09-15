using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserAuthJwt.Application.Models;
using UserAuthJwt.Infrastructure.Services;

namespace UserAuthJwt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserDapperService _userDapperService;
        private readonly IConfiguration _config;

        public UserController(ILogger<UserController> logger, IUserDapperService userDapperService, IConfiguration config)
        {
            _logger = logger;
            _userDapperService = userDapperService;
            _config = config;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var contacts = await _userDapperService.GetAllUsers();
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                // Authenticate the user
                var user = await _userDapperService.Authenticate(model.Username, model.Password);

                // If authentication fails, return unauthorized
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                // Create JWT token handler
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

                // Define the token descriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.RoleName) // Add role claim
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _config["Jwt:Issuer"],
                    Audience = _config["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                // Generate the token
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Return both token and role in the response
                return Ok(new
                {
                    Token = tokenString,
                    Role = user.RoleName,
                    Name = user.Username
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Handle specific authentication failure, for example
                return Unauthorized(new { message = "Access denied." });
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors and log them
                // You might use a logger here to log the exception details
                // _logger.LogError(ex, "An error occurred while processing the login request.");

                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var user = await _userDapperService.Register(model);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user registration.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _userDapperService.DeleteUser(userId);
                return Ok(new { message = "User and associated contact deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
