using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.settings.Enums;

namespace starsky.foundation.settings.Interfaces;

public interface ISettingsService
{
	Task<SettingsItem?> GetSetting(SettingsType key);
	Task<T?> GetSetting<T>(SettingsType key);
	Task<SettingsItem?> AddOrUpdateSetting(SettingsType key, string value);
}
