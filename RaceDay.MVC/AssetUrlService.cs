namespace RaceDay.MVC.Services
{
    // Turns image URLs returned by the API into addresses the browser can load.
    // Azure Blob URLs are already absolute; local-storage URLs are relative to the API.
    public class AssetUrlService
    {
        private readonly string _publicApiUrl;

        public AssetUrlService(IConfiguration configuration)
        {
            _publicApiUrl = (configuration["ApiPublicUrl"] ?? "http://localhost:5100").TrimEnd('/');
        }

        public string? Resolve(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            return url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? url
                : _publicApiUrl + (url.StartsWith('/') ? url : "/" + url);
        }
    }
}
