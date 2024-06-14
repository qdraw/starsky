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

	public async Task SetupAsync(string[] args)
	{
		await SetupAsync(ArgsHelper.GetRuntime(args));
	}

	public async Task SetupAsync(List<(OSPlatform?, Architecture?)> currentPlatforms)
	{
		await RunMigrations.Run(_dbContext, _logger, _appSettings);
		

		if ( currentPlatforms.Count == 0 )
		{
			currentPlatforms =
			[
				( PlatformParser.GetCurrentOsPlatform(),
					PlatformParser.GetCurrentArchitecture() )
			];
		}

		foreach ( var (osPlatform, _) in currentPlatforms )
		{
			await _exifToolDownload.DownloadExifTool(osPlatform == OSPlatform.Windows);
		}

		await _geoFileDownload.DownloadAsync();
	}
}
