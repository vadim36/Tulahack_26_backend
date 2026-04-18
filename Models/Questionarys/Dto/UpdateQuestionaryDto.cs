using backend.Models.Share;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Questionarys.Dto
{
    public class UpdateQuestionaryDto
    {
        [Required]
        public Guid UserId { get; set; }
        public IFormFile? Image { get; set; }
        public string? Name { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        public Gender? UserGender { get; set; }
        public int? Age { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public IEnumerable<string>? AllergicToPetIds { get; set; }
        public IEnumerable<string>? WantToPetIds { get; set; }
        public Gender? PetGender { get; set; }
        public int? ageFrom { get; set; }
        public int? ageTo { get; set; }
    }
}
