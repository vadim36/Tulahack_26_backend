using backend.Models.Share;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Pets.Dto
{
    public class CreatePetDto
    {
        [Required]
        public IFormFile Image { get; set; }

        [Required]
        public Guid PetTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        [Required]
        [MinLength(1)]
        public IEnumerable<string> TagIds { get; set; }

        [Required]
        public int Age { get; set; }
        [Required]
        public Gender PetGender { get; set; }
        [Required]
        public float Weight { get; set; }
        [Required]
        public DateOnly Birthday { get; set; }

        [Required]
        public float EnergyRating { get; set; }
        [Required]
        public float FriendlyRating { get; set; }
        [Required]
        public float ObedienceRating { get; set; }
        [Required]
        public float HealthRating { get; set; }
        [Required]
        public string HealthDescription { get; set; }
    }
}
