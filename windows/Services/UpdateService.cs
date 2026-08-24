using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Starsky.Desktop.Services;

public class UpdateService
{
    private const int SuppressMinutes = 5760; // 4 days
    private readonly SettingsService _settings;
    private readonly HttpClient _http;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(SettingsService settings, ILogger<UpdateService> logger)
    {
        _settings = settings;
        _logger = logger;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task<bool> CheckAsync(string baseUrl, string version)
    {
        if (!_settings.Current.UpdateCheckEnabled)
        {
	        return false;
        }

        if (_settings.Current.LastUpdateWarningShown.HasValue)
        {
            var minutesSince = (DateTime.UtcNow - _settings.Current.LastUpdateWarningShown.Value).TotalMinutes;
            if (minutesSince < SuppressMinutes)
            {
	            return false;
            }
        }

        try
        {
            var response = await _http.GetAsync($"{baseUrl.TrimEnd('/')}/api/health/check-for-updates?currentVersion={Uri.EscapeDataString(version)}");
            return response.StatusCode == System.Net.HttpStatusCode.Accepted; // 202
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Update check failed");
            return false;
        }
    }

    public void RecordWarningShown()
    {
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow;
        _settings.Save();
    }
}
