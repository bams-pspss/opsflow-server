using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OpsFlow.Data;
using OpsFlow.Models;

namespace OpsFlow.Services
{
    public class AuthService(DataContextEF context, IConfiguration config)
    {
        private readonly IConfiguration _config = config;
        private readonly DataContextEF _context = context;

        public async Task<string> GenerateJwt(User user, string roleName)
        {
            //Before sending the Claim we need to generate the role of the user
            var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Id == user.RoleId);
            //Create Claims (info inside the token)
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName)

                };

            //Create Signing Keys = 
            var key = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            //Create credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Create the token = combine everything
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}