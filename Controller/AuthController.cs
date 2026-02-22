using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using OpsFlow.Data;
using OpsFlow.Models;
using OpsFlow.Dtos;

namespace OpsFlow.Controller
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController(DataContextEF context) : ControllerBase
    {
        //Setup the datacontext
        private readonly DataContextEF _context = context;

        [HttpGet("getuser")]
        public async Task<IEnumerable<User>> TestingGetUser()
        {
            return await _context.Users.ToListAsync();
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            bool isMatch = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);

            if (!isMatch)
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.RoleId
            });
        }
    }


}