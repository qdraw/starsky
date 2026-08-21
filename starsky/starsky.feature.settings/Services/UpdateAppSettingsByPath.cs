using System.Collections.Generic;
using System.Text.Json;
using starsky.feature.settings.Interfaces;
using starsky.feature.settings.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.feature.settings.Services;

[Service(typeof(IUpdateAppSettingsByPath), InjectionLifetime = InjectionLifetime.Scoped)]
public class UpdateAppSettingsByPath : IUpdateAppSettingsByPath
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostStorage;
	private readonly IDiskWatcher _diskWatcher;

	public UpdateAppSettingsByPath(AppSettings appSettings, ISelectorStorage selectorStorage,
		IDiskWatcher diskWatcher)
	{
		_appSettings = appSettings;
		_hostStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_diskWatcher = diskWatcher;
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

		var previousMappings = new Dictionary<string, string>(_appSettings.StorageFolderMappings);
		
		AppSettingsCompareHelper.Compare(_appSettings, appSettingTransferObject);
		var transfer = ( AppSettingsTransferObject ) _appSettings;

		// should not forget app: prefix
		var jsonOutput = JsonSerializer.Serialize(new { app = transfer },
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		await _hostStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream(jsonOutput),
			_appSettings.AppSettingsPath);

		// Notify DiskWatcher about new mappings
		NotifyDiskWatcherOfNewMappings(previousMappings, _appSettings.StorageFolderMappings);

		return new UpdateAppSettingsStatusModel { StatusCode = 200, Message = "Updated" };
	}

	private void NotifyDiskWatcherOfNewMappings(Dictionary<string, string> previousMappings,
		Dictionary<string, string> currentMappings)
	{
		foreach ( var mapping in currentMappings )
		{
			if ( !previousMappings.ContainsKey(mapping.Key) )
			{
				_diskWatcher.Watcher(mapping.Value);
			}
		}
	}
}
