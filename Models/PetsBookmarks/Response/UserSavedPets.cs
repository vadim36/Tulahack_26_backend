using backend.Models.PetsBookmarks;

namespace backend.Models.PetsBookmarks.Response
{
    public class UserSavedPets
    {
        public IEnumerable<PetBookmark> Pets { get; set; }
    }
}
