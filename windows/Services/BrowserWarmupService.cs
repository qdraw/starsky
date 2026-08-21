using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace Starsky.Windows.Services;

public sealed class BrowserWarmupService
{
    private readonly HttpClient _httpClient;

    public BrowserWarmupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "1.0";

    public async Task<bool> WaitForServerAsync(Uri baseUri, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 300; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await _httpClient.GetAsync(new Uri(baseUri, "/api/health"), cancellationToken);
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable)
                {
                    var text = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (text.Contains("ealth", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            await Task.Delay(200, cancellationToken);
        }

        return false;
    }

    public async Task<bool?> CheckVersionAsync(Uri baseUri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            new Uri(baseUri, $"/api/health/version?version={Uri.EscapeDataString(AppVersion)}"));
        request.Headers.Add("x-api-version", AppVersion);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.BadRequest => false,
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ShouldShowUpdateWarningAsync(Uri baseUri, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri(baseUri,
                $"/api/health/check-for-updates?currentVersion={Uri.EscapeDataString(AppVersion)}");
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            return response.StatusCode == HttpStatusCode.Accepted;
        }
        catch
        {
            return false;
        }
    }
}