using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProjectsManager.Authentication.Models;
using ProjectsManager.Database;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectsManager.Authentication
{
    public class CreateJwtToken
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CreateJwtToken(JwtConfig jwtConfig, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _jwtConfig = jwtConfig;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);

            var claims = new List<Claim>(){
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var rol in roles)
                claims.Add(new Claim(ClaimTypes.Role, rol));
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }
    }
}
