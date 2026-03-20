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
    [Route("api/projects/{projectId}/tasks")]
    [Authorize]
    public class TaskController(DataContextEF context, IConfiguration config) : ControllerBase
    {

        private readonly DataContextEF _context = context;
        private readonly IConfiguration _config = config;

        // 1️⃣ Create Task
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask(int projectId, NewTaskDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var userIdInt = int.Parse(userId);

            var isMember = await _context.ProjectMembers
                .AnyAsync(pm =>
                    pm.ProjectId == projectId &&
                    pm.UserId == userIdInt);

            if (!isMember)
                return Forbid();

            if (dto.AssignedUserId != null)
            {
                var isAssignedValid = await _context.ProjectMembers
                    .AnyAsync(pm =>
                        pm.ProjectId == projectId &&
                        pm.UserId == dto.AssignedUserId);

                if (!isAssignedValid)
                    return BadRequest("Assigned user is not in this project.");
            }
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                ProjectId = projectId, // ⭐ from URL
                AssignedUserId = null,
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok("New Task Added!");
        }



        // 4️⃣ Get Tasks By ProjectId
        [HttpGet]
        public async Task<IActionResult> GetTasks(int projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            return Ok(tasks); // empty list is OK
        }

        // 5️⃣ Update Task
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int projectId, int taskId, UpdateTaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null || task.ProjectId != projectId)
                return NotFound("Task not found.");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();

            return Ok(task);
        }


        // 6️⃣ Delete Task
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int projectId, int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null || task.ProjectId != projectId)
                return NotFound("Task not found.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Task deleted successfully.");
        }
    }
}