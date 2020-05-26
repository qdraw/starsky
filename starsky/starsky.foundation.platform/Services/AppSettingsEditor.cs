using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Services
{
	[Service(typeof(IAppSettingsEditor), InjectionLifetime = InjectionLifetime.Scoped)]
	public class AppSettingsEditor : IAppSettingsEditor
	{
		private readonly AppSettings _sourceAppSettings;
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
