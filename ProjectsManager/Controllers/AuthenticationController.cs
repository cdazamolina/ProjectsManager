using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectsManager.Authentication;
using ProjectsManager.Authentication.Models;
using ProjectsManager.Core.Users.Register;
using ProjectsManager.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly JwtConfig _jwtConfig;
        private readonly ApiDbContext _context;

        public AuthenticationController(UserManager<User> userManager,
                               RoleManager<Roles> roleManager,
                               IOptionsMonitor<JwtConfig> jwtConfig,
                               ApiDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.CurrentValue;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AuthRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }

            var existingUser = _context.Users.FirstOrDefault(x => x.UserName == user.Username);
            if (existingUser == null)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid authentication request" }
                });
            }

            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);
            if (isCorrect)
            {
                var tokenCreator = new CreateJwtToken(_jwtConfig, _userManager, _roleManager);
                var jwtToken = await tokenCreator.GenerateJwtToken(existingUser);

                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = jwtToken
                });
            }
            else
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid authentication request" }
                });
            }
        }
    }
}
