
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Result")]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int Position { get; set; }

        [Required]
        public int RegistrationID { get; set; }
    }
}
