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


namespace OpsFlow.Controller
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController(DataContextEF context, IConfiguration config) : ControllerBase
    {
        //Setup the datacontext
        private readonly DataContextEF _context = context;
        private readonly IConfiguration _config = config;

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
            //Check if the email is there
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already exists.");
            }

            //Using lib called bcrypt = to salt and has it automatically
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                PasswordHash = passwordHash,
                RoleId = 3
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            // 1) Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            // 2) Verify password
            bool isMatch = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);

            if (!isMatch)
                return Unauthorized("Invalid credentials.");


            //Before sending the Claim we need to generate the role of the user
            var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Id == user.RoleId);
            // 3) Create claims (info stored inside token)
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role!.Name)
                };

            // 4) Create signing key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            // 5) Create credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 6) Create the token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            // 7) Convert token object to string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // 8) Return token to client
            return Ok(new
            {
                token = jwt,
                expiresInMinutes = 60,
                user = new { user.Id, user.Email, user.RoleId }
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