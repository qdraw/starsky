using System.Text.Json;
using starsky.feature.settings.Helpers;
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
public class UpdateAppSettingsByPath(
	AppSettings appSettings,
	ISelectorStorage selectorStorage,
	IDiskWatcher diskWatcher)
	: IUpdateAppSettingsByPath
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);


	public async Task<UpdateAppSettingsStatusModel> UpdateAppSettingsAsync(
		AppSettingsTransferObject appSettingTransferObject)
	{
		if ( !string.IsNullOrEmpty(appSettingTransferObject.StorageFolder) )
		{
			if ( !appSettings.StorageFolderAllowEdit )
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

		foreach ( var (_, physicalPath) in appSettingTransferObject.StorageFolderMappings )
		{
			if ( RestrictedPath.IsRestrictedPath(physicalPath) )
			{
				return new UpdateAppSettingsStatusModel
				{
					StatusCode = 403,
					Message =
						$"Mapping target '{physicalPath}' points to a restricted system directory"
				};
			}
		}

		var previousMappings = new Dictionary<string, string>(appSettings.StorageFolderMappings);

		AppSettingsCompareHelper.Compare(appSettings, appSettingTransferObject);
		var transfer = ( AppSettingsTransferObject ) appSettings;

		// should not forget app: prefix
		var jsonOutput = JsonSerializer.Serialize(new { app = transfer },
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		await _hostStorage.WriteStreamAsync(
			StringToStreamHelper.StringToStream(jsonOutput),
			appSettings.AppSettingsPath);

		// Notify DiskWatcher about new mappings
		NotifyDiskWatcherOfNewMappings(previousMappings, appSettings.StorageFolderMappings);

		return new UpdateAppSettingsStatusModel { StatusCode = 200, Message = "Updated" };
	}

	private void NotifyDiskWatcherOfNewMappings(Dictionary<string, string> previousMappings,
		Dictionary<string, string> currentMappings)
	{
		foreach ( var mapping in
		         currentMappings.Where(mapping =>
			         !previousMappings.ContainsKey(mapping.Key)) )
		{
			diskWatcher.Watcher(mapping.Value);
		}
	}
}
