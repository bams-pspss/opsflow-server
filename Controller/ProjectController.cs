using System.Collections;
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
        public async Task<IActionResult> NewProject(NewProjectDtos dto)
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

        //Get ALL Project
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.Projects.ToListAsync();
            return Ok(projects);
        }

        //Get Project By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
                return NotFound("Project not found.");

            return Ok(project);
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