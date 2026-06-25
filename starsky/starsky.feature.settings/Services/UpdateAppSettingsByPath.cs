using System.Text.Json;
using starsky.feature.settings.Interfaces;
using starsky.feature.settings.Models;
using starsky.foundation.injection;
using starsky.foundation.native.FileSystem;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.settings.Services;

[Service(typeof(IUpdateAppSettingsByPath), InjectionLifetime = InjectionLifetime.Scoped)]
public class UpdateAppSettingsByPath : IUpdateAppSettingsByPath
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostStorage;
	private readonly IWebLogger _logger;

	public UpdateAppSettingsByPath(AppSettings appSettings,
		ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_appSettings = appSettings;
		_hostStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
	}

	public async Task<UpdateAppSettingsStatusModel> UpdateAppSettingsAsync(
		AppSettingsTransferObject appSettingTransferObject)
	{
		if ( !string.IsNullOrEmpty(appSettingTransferObject.StorageFolder) )
		{
			if ( !_appSettings.StorageFolderAllowEdit )
			{
				return new UpdateAppSettingsStatusModel
				{
					StatusCode = 403,
					Message =
						"There is an Environment variable set so you can't update it here"
				};
			}

			_ = new MacOsSecurityScopedBookmark(_logger).TryStartAccessFromToken(
				appSettingTransferObject.StorageFolder!,
				appSettingTransferObject.StorageFolderToken);

			if ( !_hostStorage.ExistFolder(appSettingTransferObject.StorageFolder) )
			{
				return new UpdateAppSettingsStatusModel
				{
					StatusCode = 404,
					Message =
						"Location of StorageFolder on disk not found"
				};
			}
		}

		AppSettingsCompareHelper.Compare(_appSettings, appSettingTransferObject);
		var transfer = ( AppSettingsTransferObject ) _appSettings;

		// should not forget app: prefix
		var jsonOutput = JsonSerializer.Serialize(new { app = transfer },
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		await _hostStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream(jsonOutput),
			_appSettings.AppSettingsPath);

		return new UpdateAppSettingsStatusModel { StatusCode = 200, Message = "Updated" };
	}
}
