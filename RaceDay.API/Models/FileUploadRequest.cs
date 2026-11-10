namespace RaceDay.API.Models
{
    public class FileUploadRequest // This class is used to represent a file upload request in the API
    {
        public IFormFile File { get; set; } = null!;
    }
}
