using backend.Models.PetsType;
using backend.Models.Share;

namespace backend.Models.Questionarys
{
    public class Questionary
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Gender UserGender { get; set; }
        public int Age { get; set; }
        public string Bio { get; set; }
        public string City { get; set; }
        public IEnumerable<PetType> AllergicToPets { get; set; }
        public IEnumerable<PetType> WantToPets { get; set; }
        public Gender PetGender { get; set; }
        public int ageFrom { get; set; }
        public int ageTo { get; set; }
    }
}
