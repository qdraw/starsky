namespace Starsky.Desktop.Services;

public class NavigationService(SettingsService settings)
{
	public static bool IsAllowedOrigin(Uri uri, string baseUrl)
    {
        if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
	        return true;
        }

        return !string.IsNullOrEmpty(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
                                              && uri.Host.Equals(baseUri.Host, StringComparison.OrdinalIgnoreCase)
                                              && uri.Scheme.Equals(baseUri.Scheme, StringComparison.OrdinalIgnoreCase)
                                              && uri.Port == baseUri.Port;
    }

    public static string BuildStartUrl(string baseUrl, string? route)
    {
        var cleanBase = baseUrl.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(route))
        {
	        route = "?f=/";
        }

        if (!route.StartsWith('/') && !route.StartsWith('?'))
        {
	        route = "/" + route;
        }

        return cleanBase + route;
    }

    public string GetEffectiveBaseUrl(int? localPort = null)
    {
        if (settings.Current.Mode == Models.RuntimeMode.Local && localPort.HasValue)
        {
	        return $"http://localhost:{localPort.Value}";
        }

        return settings.Current.RemoteBaseUrl.TrimEnd('/');
    }
}
