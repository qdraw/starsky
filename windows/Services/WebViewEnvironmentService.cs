using System.Diagnostics.CodeAnalysis;
using Microsoft.Web.WebView2.Core;

namespace Starsky.Desktop.Services;

[ExcludeFromCodeCoverage]
public class WebViewEnvironmentService
{
    private CoreWebView2Environment? _environment;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<CoreWebView2Environment> GetEnvironmentAsync()
    {
        if (_environment != null)
        {
	        return _environment;
        }

        await _lock.WaitAsync();
        try
        {
            _environment ??= await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: ApplicationPaths.WebView2UserData);
        }
        finally
        {
            _lock.Release();
        }

        return _environment;
    }
}
