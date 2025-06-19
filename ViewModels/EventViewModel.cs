// Hypothetical EventViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace EventEase.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        // Existing Venue properties
        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }
        public List<SelectListItem>? VenueList { get; set; }

        // Properties for Image upload
        [Display(Name = "Event Image")]
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        // ADD THESE PROPERTIES FOR EVENTTYPE
        [Required]
        [Display(Name = "Event Type")]
        public int EventTypeId { get; set; } // To bind the selected EventType
        public List<SelectListItem>? EventTypeList { get; set; } // To populate the dropdown list
    }
}