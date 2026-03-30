using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.import.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;

namespace starsky.foundation.import.Services;

[Service(typeof(IMountWatcherLogWriter), InjectionLifetime = InjectionLifetime.Singleton)]
public class MountWatcherLogWriter(AppSettings appSettings) : IMountWatcherLogWriter
{
	private readonly SemaphoreSlim _mutex = new(1, 1);

	public async Task WriteAsync(string eventName, object payload,
		CancellationToken cancellationToken = default)
	{
		var logPath = GetLogPath(appSettings);
		var folder = Path.GetDirectoryName(logPath);
		if ( !string.IsNullOrWhiteSpace(folder) )
		{
			Directory.CreateDirectory(folder);
		}

		var line = JsonSerializer.Serialize(new
		{
			timestampUtc = DateTime.UtcNow,
			eventName,
			payload
		});

		await _mutex.WaitAsync(cancellationToken);
		try
		{
			await File.AppendAllTextAsync(logPath, line + Environment.NewLine, cancellationToken);
		}
		finally
		{
			_mutex.Release();
		}
	}

	internal static string GetLogPath(AppSettings appSettings)
	{
		if ( !string.IsNullOrWhiteSpace(appSettings.MountWatcherLogPath) )
		{
			return appSettings.MountWatcherLogPath;
		}

		return Path.Combine(appSettings.BaseDirectoryProject, "temp", "starsky-mount-watcher.log");
	}
}

