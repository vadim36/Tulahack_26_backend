using backend.Models.Pets;
using backend.Models.Questionarys;
using backend.Models.Share;

namespace backend.Tools
{
    public class XTools
    {
        public bool ApplyFilters(Pet pet, Questionary questionary)
        {
            if (pet.PetHealth != null)
            {
                if (pet.PetHealth.Age < questionary.ageFrom || pet.PetHealth.Age > questionary.ageTo)
                    return false;
            }

            if (questionary.AllergicToPets != null &&
                questionary.AllergicToPets.Contains(pet.PetType))
                return false;

            if (questionary.WantToPets != null &&
                questionary.WantToPets.Any() &&
                !questionary.WantToPets.Contains(pet.PetType))
                return false;

            if (pet.PetHealth != null &&
                pet.PetHealth.PetGender != questionary.PetGender)
                return false;

            return true;
        }
    }
}
