using backend.Models.Questionarys;

namespace backend.Models.Auth
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public string SubscriptionTier { get; set; }

        public Questionary Questionary { get; set; }

        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
