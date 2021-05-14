using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectsManager.Authentication.Models;
using ProjectsManager.Core.Projects.Update;
using ProjectsManager.Database;
using ProjectsManager.Services.Mailer;

namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator,Operator", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly EmailSender _mailer;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProjectsController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApiDbContext context, IOptions<MailSettings> mailSettings)
        {
            _context = context;
            _mailer = new EmailSender(mailSettings);
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Return a list of projects. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        /// <summary>
        /// Return detail about a specific project. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(x => x.ProjectTasks)
                .FirstOrDefaultAsync(x=> x.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        /// <summary>
        /// Request to finish a project in progress.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/Finish")]
        public async Task<IActionResult> FinishProject(int id)
        {
            Project project = await _context.Projects
                .Include(x => x.ProjectTasks)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (project == null)
            {
                return NotFound(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Project not found" }
                });
            }

            if (project.ProjectTasks.Any(x => x.Status == ProjectTask.IN_PROGRESS))
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { $"Error, some tasks are IN_PROGRESS.." }
                });
            }

            project.Status = Project.FINISHED;
            await _context.SaveChangesAsync();

            var template = _mailer.HTMLTemplate($"El proyecto {project.Name} se ha finalizado con éxito, buen trabajo.");
            var admins = await _userManager.GetUsersInRoleAsync("Administrator");

            List<Task> tasksMailer = new List<Task>();

            foreach (var admin in admins) tasksMailer.Add(_mailer.SendEmailAsync( admin.Email, "FINALIZACIÓN DE PROYECTO", template));

            Task.WaitAll(tasksMailer.ToArray());
            return Ok(project);
        }

        /// <summary>
        /// Request to update information about a project.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="projectUpdate"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, [FromBody] ProjectUpdateRequest projectUpdate)
        {
            Project project = await _context.Projects
                .Include(x => x.ProjectTasks)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (project == null)
            {
                return NotFound(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Project not found" }
                });
            }

            if (projectUpdate.EndDateTime != default && projectUpdate.EndDateTime != project.EndDateTime)
            {
                if ((from task in project.ProjectTasks where DateTime.Compare(projectUpdate.EndDateTime, task.ExecutionDateTime) < 0 select task).Any())
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>() { "Error, somes project tasks will be execute after your new project end-date, we can't assign the new end-date." }
                    });
                }

                if (DateTime.Compare(projectUpdate.EndDateTime, project.StartDateTime) > 0)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>() { "Error, new end-date is early to startDate." }
                    });
                }

                project.EndDateTime = projectUpdate.EndDateTime; 
            }

            if (projectUpdate.Name != null && projectUpdate.Name != project.Name)
            {
                project.Name = projectUpdate.Name;
            }

            if (projectUpdate.Description != null && projectUpdate.Description != project.Description)
            {
                project.Description = projectUpdate.Description;
            }

            await _context.SaveChangesAsync();
            return Ok(project);
        }

        /// <summary>
        /// Request to create a new project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }

            if (DateTime.Compare(project.StartDateTime, DateTime.Today) < 0)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Project start date-time must be greather than current date-time" }
                });
            }

            if (DateTime.Compare(project.EndDateTime, project.StartDateTime) < 0)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Project end date-time must be greather than start date-time" }
                });
            }

            project.Status = Project.IN_PROGRESS;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }

        /// <summary>
        /// Request to Delete a project
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();
            
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
