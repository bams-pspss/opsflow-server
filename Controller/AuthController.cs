using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using OpsFlow.Data;
using OpsFlow.Models;
using OpsFlow.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using OpsFlow.Services;


namespace OpsFlow.Controller
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(DataContextEF context, IConfiguration config, AuthService authService) : ControllerBase
    {
        //Setup the datacontext
        private readonly DataContextEF _context = context;
        private readonly IConfiguration _config = config;
        private readonly AuthService _authService = authService;


        //Get user Profile through Token
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { userId, role });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            //ensure email is lowercase and check it
            dto.Email = dto.Email.Trim().ToLower();

            //Check if the email is there
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            //Using lib called bcrypt = to salt and has it automatically
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                PasswordHash = passwordHash,
                RoleId = 1, //user only
                IsActive = true
            };

            //Add user into database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //Before sending the Claim we need to generate the role of the user
            var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Id == user.RoleId);

            //Create TOKEN!
            var jwt = _authService.GenerateJwt(user, role!.Name);

            return Ok(new
            {
                token = jwt,
                expiresInMinutes = 60,
                user = new { user.Id, user.Name, user.Email, user.RoleId }
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            // 1) Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });
            // 2) Verify password
            bool isMatch = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);

            if (!isMatch)
                return Unauthorized(new { message = "Invalid credentials." });

            //Before sending the Claim we need to generate the role of the user
            var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Id == user.RoleId);

            //Create TOKEN!
            var jwt = _authService.GenerateJwt(user, role!.Name);

            return Ok(new
            {
                token = jwt,
                expiresInMinutes = 60,
                user = new { user.Id, user.Name, user.Email, user.RoleId }
            });
        }

        [Authorize]
        [HttpGet("secure")]
        public IActionResult Secure()
        {
            return Ok("You are Authorize");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("AdminOnly")]
        public IActionResult AdminOnly()
        {
            return Ok("You are Admin!");
        }

        [Authorize]
        [HttpGet("debug-role")]
        public IActionResult DebugRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(role);
        }


    }



}