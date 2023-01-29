using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers;

public static class PortProgramHelper
{
	public static async Task SetEnvPortAspNetUrlsAndSetDefault(string[] args)
	{
		await SetEnvPortAspNetUrls(args, Path.Join(new AppSettings().BaseDirectoryProject, "appsettings.json"));
		SetDefaultAspNetCoreUrls(args);
	}

	public static async Task SetEnvPortAspNetUrls(IEnumerable<string> args, string appSettingsPath)
	{
		// Set port from environment variable
		var port = Environment.GetEnvironmentVariable("PORT");

		var appContainer = await ReadAppSettings.Read(appSettingsPath);
		if ( appContainer.Kestrel?.Endpoints?.Http?.Url != null || appContainer.Kestrel?.Endpoints?.Https?.Url != null)	
		{
			return;
		}

		if (args.Contains("--urls") ||string.IsNullOrEmpty(port) && !int.TryParse(port, out _)) return;
		Console.WriteLine($"Set port from environment variable: {port}");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://*:{port}");
	}
	
	public static void SetDefaultAspNetCoreUrls(IEnumerable<string> args)
	{
		var aspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
		if (args.Contains("--urls") || !string.IsNullOrEmpty(aspNetCoreUrls)) return;
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:4000;https://localhost:4001");
	}
}
