using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public HttpStatusCode Status { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsUnauthorized => Status == HttpStatusCode.Unauthorized;
    }

    // Typed HttpClient wrapper. It forwards the API's session cookie (stored in the MVC session)
    // so the API recognises the logged-in user, and it converts failures into friendly messages.
    public class ApiClient
    {
        private const string CookieKey = "ApiCookie";
        private const string ApiSessionCookieName = ".AspNetCore.Session=";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<ApiClient> logger)
        {
            _http = http;
            _accessor = accessor;
            _logger = logger;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public Task<ApiResult<T>> GetAsync<T>(string path) =>
            SendAsync<T>(HttpMethod.Get, path, null);

        public Task<ApiResult<T>> PostAsync<T>(string path, object body) =>
            SendAsync<T>(HttpMethod.Post, path, JsonContent(body));

        public Task<ApiResult<T>> PutAsync<T>(string path, object body) =>
            SendAsync<T>(HttpMethod.Put, path, JsonContent(body));

        public Task<ApiResult<object>> DeleteAsync(string path) =>
            SendAsync<object>(HttpMethod.Delete, path, null);

        public Task<ApiResult<object>> PostEmptyAsync(string path) =>
            SendAsync<object>(HttpMethod.Post, path, new StringContent(string.Empty));

        // Sends an image to the API (the API stores it in Azure Blob Storage).
        public async Task<ApiResult<object>> PostFileAsync(string path, IFormFile file)
        {
            var form = new MultipartFormDataContent();
            var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            form.Add(fileContent, "file", file.FileName);

            return await SendAsync<object>(HttpMethod.Post, path, form);
        }

        private static StringContent JsonContent(object body) =>
            new(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

        private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, HttpContent? content)
        {
            try
            {
                using var request = new HttpRequestMessage(method, path) { Content = content };

                string? cookie = Session.GetString(CookieKey);
                if (!string.IsNullOrEmpty(cookie))
                {
                    request.Headers.Add("Cookie", cookie);
                }

                using var response = await _http.SendAsync(request);
                CaptureCookie(response);

                string body = await response.Content.ReadAsStringAsync();

                var result = new ApiResult<T>
                {
                    Status = response.StatusCode,
                    Success = response.IsSuccessStatusCode
                };

                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrWhiteSpace(body) && typeof(T) != typeof(object))
                    {
                        result.Data = JsonSerializer.Deserialize<T>(body, JsonOptions);
                    }

                    result.Message = ReadMessage(body) ?? string.Empty;
                }
                else
                {
                    result.Message = FriendlyError(response.StatusCode, body);
                }

                return result;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "API call failed: {Method} {Path}", method, path);

                return new ApiResult<T>
                {
                    Success = false,
                    Status = HttpStatusCode.ServiceUnavailable,
                    Message = "We could not reach the RaceDay service. Please try again in a moment."
                };
            }
        }

        private void CaptureCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                return;
            }

            foreach (string value in values)
            {
                if (value.StartsWith(ApiSessionCookieName, StringComparison.Ordinal))
                {
                    Session.SetString(CookieKey, value.Split(';')[0]);
                }
            }
        }

        private static string? ReadMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);

                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString();
                }
            }
            catch (JsonException)
            {
            }

            return null;
        }

        // Never show raw API/server errors to the user.
        private static string FriendlyError(HttpStatusCode status, string body)
        {
            string? message = ReadMessage(body);

            if (!string.IsNullOrWhiteSpace(message))
            {
                return message;
            }

            return status switch
            {
                HttpStatusCode.BadRequest => "Some of the details you entered are not valid. Please check them and try again.",
                HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
                HttpStatusCode.Forbidden => "You do not have permission to do that.",
                HttpStatusCode.NotFound => "We could not find what you were looking for.",
                HttpStatusCode.Conflict => "That action conflicts with existing information.",
                _ => "Something went wrong on our side. Please try again."
            };
        }
    }
}
