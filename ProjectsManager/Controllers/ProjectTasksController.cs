using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectsManager.Core.ProjectTasks.Update;
using ProjectsManager.Database;

namespace ProjectsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator,Operator", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectTasksController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ProjectTasksController(ApiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Return a specific projectTask information.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTask>> GetProjectTask(int id)
        {
            var projectTask = await _context.ProjectTasks.FindAsync(id);

            if (projectTask == null)
            {
                return NotFound();
            }

            return projectTask;
        }

        /// <summary>
        /// Request to finish a current project task in progress.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/Finish")]
        public async Task<IActionResult> FinishTask(int id)
        {
            ProjectTask task = await _context.ProjectTasks
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return NotFound("Project task not found");

            task.Status = ProjectTask.FINISHED;
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        /// <summary>
        /// Request to update information about a project task.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newTask"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProjectTask(int id, ProjectTaskUpdateRequest newTask)
        {
            ProjectTask task = await _context.ProjectTasks
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return NotFound("Project task not found");

            if (newTask.ExecutionDate != default && newTask.ExecutionDate != task.ExecutionDateTime)
            {
                if (DateTime.Compare(newTask.ExecutionDate, task.Project.StartDateTime) < 0)
                    return BadRequest("Error, date is early to init project date.");

                if (DateTime.Compare(newTask.ExecutionDate, task.Project.EndDateTime) > 0)
                    return BadRequest("Error, date is greater than end project date.");

                task.ExecutionDateTime = newTask.ExecutionDate;
            }

            if (newTask.Name != null && newTask.Name != task.Name) task.Name = newTask.Name;
            
            if (newTask.Description != null && newTask.Description != task.Description) task.Description = newTask.Description;
            
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        /// <summary>
        /// Create a new project task for a specific project in process. 
        /// </summary>
        /// <param name="projectTask"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ProjectTask>> PostProjectTask(ProjectTask projectTask)
        {
            Project project = _context.Projects.Find(projectTask.ProjectId);
            
            if (project == null)
                return BadRequest("Invalid project Id");

            if (project.Status == Project.FINISHED)
                return BadRequest("Error, Project is finished.");
            
            if (DateTime.Compare(projectTask.ExecutionDateTime, project.StartDateTime) < 0)
                return BadRequest("Error, date is early to init project date.");

            if (DateTime.Compare(projectTask.ExecutionDateTime, project.EndDateTime) > 0)
                return BadRequest("Error, date is greater than end project date.");

            projectTask.Status = ProjectTask.IN_PROGRESS; // By Default;
            _context.ProjectTasks.Add(projectTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProjectTask", new { id = projectTask.Id }, projectTask);
        }

        /// <summary>
        /// Delete a current project task.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectTask(int id)
        {
            var projectTask = await _context.ProjectTasks.FindAsync(id);
            if (projectTask == null)
            {
                return NotFound();
            }

            _context.ProjectTasks.Remove(projectTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
