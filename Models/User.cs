namespace OpsFlow.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    }
}