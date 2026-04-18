using System.ComponentModel.DataAnnotations;

namespace backend.Models.PetCalendars.Dto
{
    public class CreateCalendarItemDto
    {
        [Required]
        public Guid CalendarId { get; set; }

        [Required]
        public TimeOnly Time { get; set; }
        [Required]
        public string Name { get; set; }
        public bool isActive { get; set; } = false;
        [Required]
        public ActionType ActionType { get; set; }
    }
}
