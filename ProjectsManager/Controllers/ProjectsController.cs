using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsManager.Core.Projects.Update;
using ProjectsManager.Database;

namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator,Operator", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ProjectsController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, [FromBody] ProjectUpdateRequest projectUpdate)
        {
            Project project = await _context.Projects
                .Include(x => x.ProjectTasks)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (project == null)
                return NotFound("Project not found");
            
            if (projectUpdate.EndDateTime != default && projectUpdate.EndDateTime != project.EndDateTime)
            {
                if ((from task in project.ProjectTasks where DateTime.Compare(projectUpdate.EndDateTime, task.ExecutionDateTime) < 0 select task).Any())
                {
                    return BadRequest("Error, somes project tasks will be execute after your new project end-date, we can't assign the new end-date.");
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

        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            if (!ModelState.IsValid)            
                return BadRequest("Invalid payload");

            if (DateTime.Compare(project.StartDateTime, DateTime.Today) < 0)
                return BadRequest("Project start date-time must be greather than current date-time");

            if (DateTime.Compare(project.EndDateTime, project.StartDateTime) < 0)
                return BadRequest("Project end date-time must be greather than start date-time");

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }

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
