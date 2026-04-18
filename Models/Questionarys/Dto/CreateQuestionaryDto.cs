using backend.Models.PetsType;
using backend.Models.Share;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Questionarys.Dto
{
    public class CreateQuestionaryDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        public Gender UserGender { get; set; }
        [Required]
        public int Age { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public IEnumerable<string> AllergicToPetIds { get; set; }
        [Required]
        public IEnumerable<string> WantToPetIds { get; set; }
        [Required]
        public Gender PetGender { get; set; }
        [Required]
        public int ageFrom { get; set; }
        [Required]
        public int ageTo { get; set; }
    }
}
