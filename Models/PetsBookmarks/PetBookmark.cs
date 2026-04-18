using backend.Models.Pets;

namespace backend.Models.PetsBookmarks
{
    public class PetBookmark
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Pet Pet { get; set; }
    }
}
