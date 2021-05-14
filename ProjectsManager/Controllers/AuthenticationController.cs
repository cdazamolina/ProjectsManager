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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtConfig _jwtConfig;
        private readonly ApiDbContext _context;

        public AuthenticationController(UserManager<User> userManager,
                               RoleManager<IdentityRole> roleManager,
                               IOptionsMonitor<JwtConfig> jwtConfig,
                               ApiDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.CurrentValue;
            _context = context;
        }

        /// <summary>
        /// This api returns a JWT Bearer token from the username and password provided.
        /// The token has a default expiration time of 1 day. Token must be incluid on all future request in format: Bearer [Token]
        /// </summary>
        /// <param name="user">Usuario y contraseña del administrado o operador.</param>
        /// <returns></returns>
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

            if (!existingUser.IsEnable)
            {
                return Forbid("Sorry, your account is disabled, please contact an administrator.");
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
