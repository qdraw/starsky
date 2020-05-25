using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Services
{
	public class AppSettingsEditor
	{
		private AppSettings _sourceAppSettings;

		public AppSettingsEditor(AppSettings appSettings)
		{
			_sourceAppSettings = appSettings;
		}
		
		public AppSettings Update(AppSettings updateAppSettings)
		{
			AppSettingsCompareHelper.Compare(_sourceAppSettings, updateAppSettings);
			return _sourceAppSettings;
		}
	}
}
