namespace SkillSprint.ClientUI.Models
{
    public sealed class UserSummary
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string[] Roles { get; set; } = System.Array.Empty<string>();
    }
}
