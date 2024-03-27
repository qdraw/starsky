using System.Runtime.InteropServices;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.dependenciescli;

public class DependenciesCliService
{
	private IExifToolDownload _exifToolDownload;
	private readonly ApplicationDbContext _dbContext;
	private IWebLogger _logger;
	private AppSettings _appSettings;

	public DependenciesCliService(IExifToolDownload exifToolDownload,
		ApplicationDbContext dbContext, IWebLogger logger, AppSettings appSettings)
	{
		_exifToolDownload = exifToolDownload;
		_dbContext = dbContext;
		_logger = logger;
		_appSettings = appSettings;
	}

	public async Task Run(OSPlatform? currentPlatform = null, Architecture? architecture = null)
	{
		currentPlatform ??= PlatformParser.GetCurrentOsPlatform();
		await RunMigrations.Run(_dbContext, _logger, _appSettings);
		await _exifToolDownload.DownloadExifTool(currentPlatform == OSPlatform.Windows);
		
	}
	

}
