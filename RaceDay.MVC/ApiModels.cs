namespace RaceDay.MVC.Models
{
    // Shapes of the JSON returned by the Part 2 API.
    public class EventDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int OrganiserID { get; set; }
        public decimal Distance { get; set; }
        public string? BannerUrl { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int EventID { get; set; }
    }

    public class OrganiserEventDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string? BannerUrl { get; set; }
        public int EnrolmentCount { get; set; }
        public int CategoryCount { get; set; }
    }

    public class EnrolmentDto
    {
        public int RegistrationID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ResultID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class MyEnrolmentDto
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MyResultDto
    {
        public int ResultID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class ProfileDto
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
