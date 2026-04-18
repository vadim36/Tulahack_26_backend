using backend.Models.Share;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Pets.Dto
{
    public class UpdatePetDto
    {
        public IFormFile? Image { get; set; }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid PetTypeId { get; set; }

        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [MinLength(1)]
        public IEnumerable<string>? TagIds { get; set; }

        public int? Age { get; set; }
        public Gender? PetGender { get; set; }
        public float? Weight { get; set; }
        public DateOnly? Birthday { get; set; }
        public float? EnergyRating { get; set; }
        public float? FriendlyRating { get; set; }
        public float? ObedienceRating { get; set; }
        public float? HealthRating { get; set; }
        public string? HealthDescription { get; set; }
    }
}
