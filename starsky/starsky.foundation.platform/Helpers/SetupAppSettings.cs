using System;
using Microsoft.Extensions.Configuration;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers
{
	public static class SetupAppSettings
	{
		/// <summary>
		/// Default appSettings.json to builder
		/// </summary>
		/// <returns>ConfigBuilder</returns>
		public static IConfigurationRoot AppSettingsToBuilder()
		{
			var appSettings = new AppSettings();
			var builder = new ConfigurationBuilder();
		    
			// to remove spaces and other signs, check help to get your name
			var appSettingsMachine =
				$"appsettings.{Environment.MachineName.ToLowerInvariant()}."; // dot here
		    
			builder
				.SetBasePath(appSettings.BaseDirectoryProject)
				.AddJsonFile("appsettings.patch.json",true)
				.AddJsonFile(appSettingsMachine + "patch.json", optional: true)
				.AddJsonFile("appsettings.json",true)
				.AddJsonFile(appSettingsMachine + "json", optional: true)
				// overwrite envs
				// current dir gives problems on linux arm
				.AddEnvironmentVariables();
			
			return builder.Build();
		}
	}
}
