using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.realtime.Enums;

namespace starsky.foundation.realtime.Interfaces;

public interface ISettingsService
{
	Task<SettingsItem?> GetSetting(SettingsType key);
	Task<T?> GetSetting<T>(SettingsType key);

	Task<SettingsItem?> AddOrUpdateSetting(SettingsItem item);
	Task<SettingsItem?> AddOrUpdateSetting(SettingsType key, string value);
}
