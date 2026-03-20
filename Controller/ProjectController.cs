using System.Collections;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpsFlow.Data;
using OpsFlow.Dtos;
using OpsFlow.Models;

namespace OpsFlow.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController(DataContextEF context, IConfiguration config) : ControllerBase
    {

        private readonly DataContextEF _context = context;
        private readonly IConfiguration _config = config;


        //Create Project
        [HttpPost]
        public async Task<IActionResult> NewProject(NewProjectDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var userIdInt = int.Parse(userId);


            //Add new Project to Proejct Table
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                DueDate = dto.DueDate
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();


            //Add the project to ProjectMember table
            var projectMem = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userIdInt,
                Role = "Owner"
            };
            _context.ProjectMembers.Add(projectMem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = project.Id,
                name = project.Name,
                description = project.Description,
                dueDate = project.DueDate
            });
        }


        //Get ALL Project by UserId
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            //Get UserId from token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var userIdInt = int.Parse(userId);

            //Query out Project by userId
            var projects = await _context.ProjectMembers
                .Where(pm => pm.UserId == userIdInt)
                .Select(pm => pm.Project)
                .ToListAsync();


            if (projects == null || projects.Count == 0)
                return NotFound(new { message = "No Projects" });

            return Ok(projects);
        }



        //Edit Project
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound("Project not found.");

            project.Name = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(project);
        }


        //Delete Project
        //Admin Only
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var userIdInt = int.Parse(userId);

            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound("Project not found.");

            // ⭐ Check system admin
            var systemRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (systemRole == "Admin")
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                return Ok("Project deleted by system admin.");
            }

            // ⭐ Check project owner
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == id &&
                    pm.UserId == userIdInt);

            if (membership == null)
                return Forbid(); // not part of project

            if (membership.Role != "Owner")
                return Forbid(); // not owner

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok("Project deleted successfully.");
        }
    }
}