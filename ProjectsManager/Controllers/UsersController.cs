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


namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly JwtConfig _jwtConfig;
        private readonly ApiDbContext _context;

        public UsersController(UserManager<User> userManager, 
                               RoleManager<Roles> roleManager, 
                               IOptionsMonitor<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="user">
        /// 
        /// </param>
        /// <returns></returns>
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

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                return BadRequest(new UserRegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { $"User with email {user.Email} already exist." }
                });
            }

            var role = (user.IsAdministrator)?
                await _roleManager.FindByNameAsync(Roles.ADMINISTRATOR):
                await _roleManager.FindByNameAsync(Roles.OPERATOR);

            var newUser = new User() { Email = user.Email, UserName = user.Username, IsEnable = true };
            var isCreated = await _userManager.CreateAsync(newUser, user.Password);
            if (isCreated.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, role.Name);
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
