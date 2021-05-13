using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectsManager.Authentication;
using ProjectsManager.Authentication.Models;
using ProjectsManager.Core.Users.Register;
using ProjectsManager.Core.Users.Update;
using ProjectsManager.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly JwtConfig _jwtConfig;
        private readonly ApiDbContext _context;
        private readonly CreateJwtToken _tokenCreator;

        public UsersController(UserManager<User> userManager, 
                               RoleManager<Roles> roleManager, 
                               IOptionsMonitor<JwtConfig> jwtConfig,
                               ApiDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.CurrentValue;
            _context = context;
            _tokenCreator = new CreateJwtToken(_jwtConfig, _userManager, _roleManager);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Get() => Ok(_context.Users);
        
        [HttpPost]
        [Authorize(Roles = "Administrator")]
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

            var existingUserEmail = await _userManager.FindByEmailAsync(user.Email);
            if (existingUserEmail != null)
            {
                return BadRequest(new UserRegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { $"User with email {user.Email} already exist." }
                });
            }

            var existingUsername = _context.Users.Any(x => x.UserName == user.Username);
            if (existingUsername)
            {
                return BadRequest(new UserRegistrationResponse()
                {
                    Result = false,
                    Errors = new List<string>() { $"User with username {user.Username} already exist." }
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
                string jwtToken = await _tokenCreator.GenerateJwtToken(newUser);

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
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Put(string id, [FromBody] ToggleUserStatus newStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }

            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("user not found");

            user.IsEnable = newStatus.IsEnable;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            string jwtToken = await _tokenCreator.GenerateJwtToken(user);
            return Ok(new AuthResult()
            {
                Result = true,
                Token = jwtToken
            });            
        }
        
        [HttpPut]
        [Authorize(Roles = "Administrator,Operator")]
        public async Task<IActionResult> Put([FromBody] ChangePassword newPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }

            User user = _context.Users.FirstOrDefault(x => x.UserName == newPassword.Username);
            if (user == null)
                return NotFound("user not found");

            bool isValidPassword = await _userManager.CheckPasswordAsync(user, newPassword.Password);
            if (!isValidPassword)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid authentication request" }
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, newPassword.Password, newPassword.NewPassword);
            if (result.Succeeded)
            {
                string jwtToken = await _tokenCreator.GenerateJwtToken(user);
                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = jwtToken
                });
            }

            return new JsonResult(new UserRegistrationResponse()
            {
                Result = false,
                Errors = new List<string>() { "Sorry, something is going wrong, please try again."}
            })
            { StatusCode = 500 };
        }

    }
}
