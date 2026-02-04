using starsky.feature.settings.Models;
using starsky.foundation.platform.Models;

namespace starsky.feature.settings.Interfaces;

public interface IUpdateAppSettingsByPath
{
	Task<UpdateAppSettingsStatusModel> UpdateAppSettingsAsync(
		AppSettingsTransferObject appSettingTransferObject);
}
