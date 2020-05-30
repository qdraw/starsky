using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Interfaces
{
	public interface IAppSettingsEditor
	{
		AppSettings Update(AppSettings updateAppSettings);
	}
}
