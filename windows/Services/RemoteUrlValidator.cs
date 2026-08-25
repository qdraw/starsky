using System.Net.Http;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;

namespace Starsky.Desktop.Services;

public class RemoteUrlValidator(ILogger<RemoteUrlValidator> logger, HttpClient? http = null)
{
    private readonly HttpClient _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    public async Task<UrlValidationResult> ValidateAsync(string url)
    {
        url = url.TrimEnd('/');

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
	        return new UrlValidationResult(false, "Invalid URL format");
        }

        if (uri.Scheme != "http" && uri.Scheme != "https")
        {
	        return new UrlValidationResult(false, "URL must use http or https");
        }

        try
        {
            var healthUri = new Uri(new Uri(url + "/"), "api/health");
            var response = await _http.GetAsync(healthUri);
            if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                return new UrlValidationResult(true, null);
            }

            return new UrlValidationResult(false, $"Server returned {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "URL validation failed for {Url}", url);
            return new UrlValidationResult(false, ex.Message);
        }
    }
}
