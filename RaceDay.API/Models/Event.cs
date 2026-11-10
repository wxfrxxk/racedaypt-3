
using System;
using System.ComponentModel.DataAnnotations; // this namespace is required for data annotations like [Key], [Required], and [StringLength]
using System.ComponentModel.DataAnnotations.Schema; // this namespace is required for the [Table] attribute

namespace RaceDay.API.Models // This namespace is used to organize the code and avoid naming conflicts
{
    [Table("Event")]
    public class Event
    {
        [Key]
        public int EventID { get; set; } // This property represents the primary key of the Event table

        [Required]
        [StringLength(100)] // This property represents the name of the event
        public string EventName { get; set; } = string.Empty; // This property represents the name of the event
        [Required]
        [StringLength(255)] // This property represents the description of the event
        public string Description { get; set; } = string.Empty; // This property represents the description of the event

        [Required]
        public DateTime EventDate { get; set; } // This property represents the date of the event

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty; // This property represents the location of the event
        [Required]
        [StringLength(20)]
        public string EventType { get; set; } = string.Empty; // This property represents the type of the event

        [Required]
        public int OrganiserID { get; set; } // This property represents the ID of the organiser

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; } // Distance of the event in kilometres

        [StringLength(500)]
        public string? BannerUrl { get; set; } // URL of the banner image stored in Azure Blob Storage
    }
}
