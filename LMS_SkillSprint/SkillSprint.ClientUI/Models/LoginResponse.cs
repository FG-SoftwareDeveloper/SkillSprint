using System;

namespace SkillSprint.ClientUI.Models
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public DateTime ExpiresAtUtc { get; set; }
        public UserSummary User { get; set; } = new();
    }
}
