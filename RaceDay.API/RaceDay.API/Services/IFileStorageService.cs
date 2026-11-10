namespace RaceDay.API.Services
{
    // Abstraction over file storage so controllers never depend on Azure directly.
    public interface IFileStorageService
    {
        // Uploads an image and returns the public URL that is saved in the database.
        Task<string> UploadImageAsync(IFormFile file, string folder);
    }
}
