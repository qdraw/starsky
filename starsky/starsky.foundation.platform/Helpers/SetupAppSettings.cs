using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]
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

			builder = SetLocalAppData(builder);

			return builder.Build();
		}

		/// <summary>
		/// Configure the PoCo the dependency injection
		/// </summary>
		/// <param name="services">services</param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static AppSettings ConfigurePoCoAppSettings(IServiceCollection services,
			IConfigurationRoot configuration)
		{
			// configs
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
            
			// Need to rebuild for AppSettings
			// ReSharper disable once ASP0000
			var serviceProvider = services.BuildServiceProvider();
            
			return serviceProvider.GetRequiredService<AppSettings>();
		}
	
		/// <summary>
		/// In the OS there is a folder to read and write,
		/// when you replace the entire application settings should not be overwritten
		/// Only if env variable app__AppSettingsPath exist
		/// </summary>
		/// <param name="builder">ConfigBuilder</param>
		/// <returns>Set the env variable `app__AppSettingsPath` to enable this feature</returns>
		private static ConfigurationBuilder SetLocalAppData(ConfigurationBuilder builder)
		{
			var appSettingsPath = Environment.GetEnvironmentVariable("app__AppSettingsPath");
			
			if ( appSettingsPath == null || !File.Exists(appSettingsPath))
			{
				return builder;
			}
			
			builder
				.SetBasePath(Directory.GetParent(appSettingsPath).FullName)
				.AddJsonFile("appsettings.json");
			
			return builder;
		}
	}
}
