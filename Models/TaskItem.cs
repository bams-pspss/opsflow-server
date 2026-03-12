namespace OpsFlow.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Todo;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}