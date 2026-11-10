
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; } // This property represents the primary key of the Category table

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public int EventID { get; set; } // This property represents the ID of the event
    }
}
