using backend.Models.PetsHealth;
using backend.Models.PetsType;
using backend.Models.Tags;

namespace backend.Models.Pets
{
    public class Pet
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public PetHealth PetHealth { get; set; }

        public string ImagePath { get; set; }

        public PetType PetType { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
    }
}
