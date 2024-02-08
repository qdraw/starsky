using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.foundation.platform.Helpers;

public static class PortProgramHelper
{
	public static async Task SetEnvPortAspNetUrlsAndSetDefault(string[] args, string appSettingsPath)
	{
		if ( await SkipForAppSettingsJsonFile(appSettingsPath) )
		{
			return;
		}

		SetEnvPortAspNetUrls(args);
		SetDefaultAspNetCoreUrls(args);
	}

	internal static async Task<bool> SkipForAppSettingsJsonFile(string appSettingsPath)
	{
		var appContainer = await ReadAppSettings.Read(appSettingsPath);
		if ( appContainer?.Kestrel?.Endpoints?.Http?.Url == null &&
			 appContainer?.Kestrel?.Endpoints?.Https?.Url == null )
		{
			return false;
		}

		Console.WriteLine("Kestrel Endpoints are set in appsettings.json, " +
						  "this results in skip setting the PORT and default " +
						  "ASPNETCORE_URLS environment variable");
		return true;
	}

	internal static void SetEnvPortAspNetUrls(IEnumerable<string> args)
	{
		// Set port from environment variable
		var portString = Environment.GetEnvironmentVariable("PORT");

		if ( args.Contains("--urls") || string.IsNullOrEmpty(portString)
			|| !int.TryParse(portString, out var port) ) return;

		SetEnvironmentVariableForPort(port);
	}

	[SuppressMessage("Usage", "S5332: Using http protocol is insecure. Use https instead.")]
	private static void SetEnvironmentVariableForPort(int port)
	{
		Console.WriteLine($"Set port from environment variable: {port} " +
						  $"\nPro tip: Its recommended to use a https proxy like nginx or traefik");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://*:{port}");
	}

	internal static void SetDefaultAspNetCoreUrls(IEnumerable<string> args)
	{
		var aspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
		if ( args.Contains("--urls") || !string.IsNullOrEmpty(aspNetCoreUrls) ) return;
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:4000;https://localhost:4001");
	}
}
