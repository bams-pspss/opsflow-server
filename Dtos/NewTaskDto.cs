using OpsFlow.Models;

namespace OpsFlow.Dtos
{
    public class NewTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }

        public int? AssignedUserId { get; set; }
    }
}