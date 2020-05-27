using System;
using System.Text.Json;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Services
{
	public class AppSettingsEditor : IAppSettingsEditor
	{
		private readonly AppSettings _appSettings;
		public AppSettingsEditor(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		public AppSettings Update(AppSettings updateAppSettings)
		{
			AppSettingsCompareHelper.Compare(_appSettings, updateAppSettings);
			var output = JsonSerializer.Serialize(_appSettings, new JsonSerializerOptions { WriteIndented = true });
			
			return _appSettings;
		}
	}
}
