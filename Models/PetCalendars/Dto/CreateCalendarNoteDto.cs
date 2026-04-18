using System.ComponentModel.DataAnnotations;

namespace backend.Models.PetCalendars.Dto
{
    public class CreateCalendarNoteDto
    {
        [Required]
        public Guid CalendarId { get; set; }
        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
    }
}
