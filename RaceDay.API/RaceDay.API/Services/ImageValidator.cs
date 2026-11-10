namespace RaceDay.API.Services
{
    // Validates uploaded images (type and size) before they are stored.
    public static class ImageValidator
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return "Please choose an image file to upload.";
            }

            if (file.Length > MaxBytes)
            {
                return "The image must be smaller than 5 MB.";
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) ||
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "Only JPG, PNG, WEBP or GIF images are allowed.";
            }

            return null;
        }
    }
}
