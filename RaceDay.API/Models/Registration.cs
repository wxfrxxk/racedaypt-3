
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Registration")]
    public class Registration
    {
        [Key]
        public int RegistrationID { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed or Cancelled
    }
}
