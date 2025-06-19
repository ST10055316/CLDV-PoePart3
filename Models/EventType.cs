using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Event Type Name")]
        public string? EventTypeName { get; set; }

        // Optional: Description for the event type
        [StringLength(250)]
        public string? Description { get; set; }
    }
}