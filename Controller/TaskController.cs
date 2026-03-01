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
    public class TaskController(DataContextEF context, IConfiguration config) : ControllerBase
    {

        private readonly DataContextEF _context = context;
        private readonly IConfiguration _config = config;

        // 1️⃣ Create Task
        [HttpPost]
        public async Task<IActionResult> CreateTask(NewTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                ProjectId = dto.ProjectId,
                AssignedUserId = dto.AssignedUserId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok("New Task Added!");
        }


        // 2️⃣ Get All Tasks
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _context.Tasks.ToListAsync();
            return Ok(tasks);
        }

        // 3️⃣ Get Task By Id
        [HttpGet("{id}")]

        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound("Project not found.");

            return Ok(task);
        }

        // 4️⃣ Get Tasks By ProjectId
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTaskByProjectId(int projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            if (tasks == null)
            {
                return NotFound("No Task found");
            }

            return Ok(tasks);
        }

        // 5️⃣ Update Task
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound("Tasks not found.");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();
            return Ok(task);
        }


        // 6️⃣ Delete Task
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound("Task not found.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Task deleted successfully.");
        }
    }
}