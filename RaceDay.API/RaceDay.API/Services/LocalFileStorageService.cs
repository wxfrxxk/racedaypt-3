namespace RaceDay.API.Services
{
    // Fallback used only when no Azure connection string is configured (local development, CI).
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _root;

        public LocalFileStorageService(string webRootPath)
        {
            _root = Path.Combine(webRootPath, "uploads");
            Directory.CreateDirectory(_root);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string folderPath = Path.Combine(_root, folder);
            Directory.CreateDirectory(folderPath);

            string fileName = $"{Guid.NewGuid():N}{extension}";
            await using var target = File.Create(Path.Combine(folderPath, fileName));
            await file.CopyToAsync(target);

            // Relative URL, served by the API's static files middleware.
            return $"/uploads/{folder}/{fileName}";
        }
    }
}
