using backend.Models.Share;

namespace backend.Models.PetsHealth
{
    public class PetHealth
    {
        public Guid Id { get; set; }
        public Guid PetId { get; set; }

        public int Age { get; set; }
        public Gender PetGender { get; set; }
        public float Weight { get; set; }
        public DateOnly Birthday { get; set; }

        public float EnergyRating { get; set; }
        public float FriendlyRating { get; set; }
        public float ObedienceRating { get; set; }
        public float HealthRating { get; set; }

        public string Description { get; set; }
    }
}
