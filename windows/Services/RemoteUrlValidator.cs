using System.Net.Http;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;

namespace Starsky.Desktop.Services;

public class RemoteUrlValidator
{
    private readonly HttpClient _http;
    private readonly ILogger<RemoteUrlValidator> _logger;

    public RemoteUrlValidator(ILogger<RemoteUrlValidator> logger, HttpClient? http = null)
    {
        _logger = logger;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

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
            var response = await _http.GetAsync($"{url}/api/health");
            if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                return new UrlValidationResult(true, null);
            }

            return new UrlValidationResult(false, $"Server returned {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "URL validation failed for {Url}", url);
            return new UrlValidationResult(false, ex.Message);
        }
    }
}
