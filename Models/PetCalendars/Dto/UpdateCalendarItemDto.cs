using System.ComponentModel.DataAnnotations;

namespace backend.Models.PetCalendars.Dto
{
    public class UpdateCalendarItemDto
    {
        [Required]
        public Guid Id { get; set; }
        public TimeOnly? Time { get; set; }
        public string? Name { get; set; }
        public bool? isActive { get; set; }
        public ActionType? ActionType { get; set; }
    }
}
