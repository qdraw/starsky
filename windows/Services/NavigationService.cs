namespace Starsky.Desktop.Services;

public class NavigationService
{
    private readonly SettingsService _settings;

    public NavigationService(SettingsService settings)
    {
        _settings = settings;
    }

    public bool IsAllowedOrigin(Uri uri, string baseUrl)
    {
        // Always allow localhost
        if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
	        return true;
        }

        // Allow configured remote origin
        if (!string.IsNullOrEmpty(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            if (uri.Host.Equals(baseUri.Host, StringComparison.OrdinalIgnoreCase) &&
                uri.Scheme.Equals(baseUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
                uri.Port == baseUri.Port)
            {
	            return true;
            }
        }

        return false;
    }

    public string BuildStartUrl(string baseUrl, string? route)
    {
        var cleanBase = baseUrl.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(route))
        {
	        route = "?f=/";
        }

        // Ensure route starts with / or ?
        if (!route.StartsWith('/') && !route.StartsWith('?'))
        {
	        route = "/" + route;
        }

        return cleanBase + route;
    }

    public string GetEffectiveBaseUrl(int? localPort = null)
    {
        if (_settings.Current.Mode == Models.RuntimeMode.Local && localPort.HasValue)
        {
	        return $"http://localhost:{localPort.Value}";
        }

        return _settings.Current.RemoteBaseUrl.TrimEnd('/');
    }
}
