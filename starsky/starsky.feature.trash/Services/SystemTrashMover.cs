using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.native.Trash.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.feature.trash.Services;

[Service(typeof(ISystemTrashMover), InjectionLifetime = InjectionLifetime.Scoped)]
public class SystemTrashMover : ISystemTrashMover
{
	private readonly AppSettings _appSettings;
	private readonly ITrashService _systemTrashService;
	private readonly ITenantSettings _tenantSettings;

	public SystemTrashMover(AppSettings appSettings, ITrashService systemTrashService,
		ITenantSettings? tenantSettings = null)
	{
		_appSettings = appSettings;
		_systemTrashService = systemTrashService;
		_tenantSettings = tenantSettings ??
		                  new TenantSettings(TenantIdHelper.DefaultTenantId, appSettings);
	}

	public bool IsEnabled()
	{
		return _appSettings.UseSystemTrash == true &&
		       _systemTrashService.DetectToUseSystemTrash();
	}

	public bool DetectToUseSystemTrash()
	{
		return _systemTrashService.DetectToUseSystemTrash();
	}

	public void Trash(IEnumerable<FileIndexItem> moveToTrash)
	{
		var fullFilePaths = moveToTrash
			.Where(p => p.FilePath != null)
			.Select(p => _tenantSettings.DatabasePathToFilePath(p.FilePath!))
			.ToList();

		_systemTrashService.Trash(fullFilePaths);
	}
}
