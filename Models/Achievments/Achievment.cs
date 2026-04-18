namespace backend.Models.Achivments
{
    public class Achievment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int AchievmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
