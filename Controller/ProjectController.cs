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
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok("New Project Added!");
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
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound("Project not found.");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok("Project deleted successfully.");
        }
    }
}