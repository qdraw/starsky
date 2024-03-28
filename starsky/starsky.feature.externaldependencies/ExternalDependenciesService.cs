using System.Runtime.InteropServices;
using starsky.feature.externaldependencies.Interfaces;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.externaldependencies;

[Service(typeof(IExternalDependenciesService), InjectionLifetime = InjectionLifetime.Scoped)]
public class ExternalDependenciesService : IExternalDependenciesService
{
	private readonly IExifToolDownload _exifToolDownload;
	private readonly ApplicationDbContext _dbContext;
	private readonly IWebLogger _logger;
	private readonly AppSettings _appSettings;
	private readonly IGeoFileDownload _geoFileDownload;

	public ExternalDependenciesService(IExifToolDownload exifToolDownload,
		ApplicationDbContext dbContext, IWebLogger logger, AppSettings appSettings,
		IGeoFileDownload geoFileDownload)
	{
		_exifToolDownload = exifToolDownload;
		_dbContext = dbContext;
		_logger = logger;
		_appSettings = appSettings;
		_geoFileDownload = geoFileDownload;
	}

	public async Task SetupAsync(List<string> args)
	{
		await SetupAsync(ArgsHelper.GetRuntime(args));
	}

	public async Task SetupAsync(OSPlatform? currentPlatform = null,
		Architecture? architecture = null)
	{
		currentPlatform ??= PlatformParser.GetCurrentOsPlatform();

		await RunMigrations.Run(_dbContext, _logger, _appSettings);
		await _exifToolDownload.DownloadExifTool(currentPlatform == OSPlatform.Windows);

		await _geoFileDownload.DownloadAsync();
	}
}
