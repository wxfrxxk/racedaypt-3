using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class LoginVm
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterVm
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name can be at most 50 characters.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name can be at most 50 characters.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15, ErrorMessage = "Phone number can be at most 15 characters.")]
        [RegularExpression(@"^[0-9+ ]{7,15}$", ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class HomeVm
    {
        public List<EventDto> Events { get; set; } = new();
        public string? Search { get; set; }
        public string? Type { get; set; }
        public decimal? MaxDistance { get; set; }
        public int TotalUpcoming { get; set; }
    }

    public class EventDetailsVm
    {
        public EventDto Event { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public HashSet<int> EnrolledCategoryIds { get; set; } = new();
    }

    public class EventFormVm
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(100, ErrorMessage = "Event name can be at most 100 characters.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(255, ErrorMessage = "Description can be at most 255 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event date")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location can be at most 100 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Display(Name = "Banner image")]
        public IFormFile? Banner { get; set; }

        public string? ExistingBannerUrl { get; set; }
    }

    public class CategoriesVm
    {
        public EventDto Event { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
    }

    public class EnrolmentsVm
    {
        public EventDto Event { get; set; } = new();
        public List<EnrolmentDto> Enrolments { get; set; } = new();
    }

    public class ProfileVm
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15, ErrorMessage = "Phone number can be at most 15 characters.")]
        [RegularExpression(@"^[0-9+ ]{7,15}$", ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
