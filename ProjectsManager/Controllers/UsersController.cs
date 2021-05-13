using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectsManager.Authentication;
using ProjectsManager.Authentication.Models;
using ProjectsManager.Core.Users.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public UsersController(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.CurrentValue;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserRegistrationRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserRegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }

            // check i the user with the same email exist
            var existingUser = await _userManager.FindByEmailAsync(user.Email);

            if (existingUser != null)
            {
                return BadRequest(new UserRegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { $"User with email {user.Email} already exist." }
                });
            }

            var newUser = new IdentityUser() { Email = user.Email, UserName = user.Username };
            var isCreated = await _userManager.CreateAsync(newUser, user.Password);
            if (isCreated.Succeeded)
            {
                var tokenCreator = new CreateJwtToken(_jwtConfig);
                string jwtToken = tokenCreator.GenerateJwtToken(newUser);

                return Ok(new UserRegistrationResponse()
                {
                    Result = true,
                    Token = jwtToken
                });
            }

            return new JsonResult(new UserRegistrationResponse()
            {
                Result = false,
                Errors = isCreated.Errors.Select(x => x.Description).ToList()
            })
            { StatusCode = 500 };
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
