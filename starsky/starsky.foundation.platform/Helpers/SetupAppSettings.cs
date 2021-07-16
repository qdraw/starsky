using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.platform.Helpers
{
	public static class SetupAppSettings
	{
		public static ServiceCollection FirstStepToAddSingleton(ServiceCollection services)
		{
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			var configurationRoot = AppSettingsToBuilder();
			services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));
			return services;
		}
		
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

		internal static async Task<AppSettings> MergeJsonFiles(string baseDirectoryProject)
		{
			// to remove spaces and other signs, check help to get your name
			var appSettingsMachine =
				$"appsettings.{Environment.MachineName.ToLowerInvariant()}."; // dot here
			
			var paths = new List<string>
			{
				Path.Combine(baseDirectoryProject, appSettingsMachine + "json"),
				Path.Combine(baseDirectoryProject, "appsettings.json"),
				Path.Combine(baseDirectoryProject, appSettingsMachine + "patch.json"),
				Path.Combine(baseDirectoryProject, "appsettings.patch.json"),
				Environment.GetEnvironmentVariable("app__AppSettingsPath")
			};

			var appSettingsList = new List<AppSettings>();

			foreach ( var path in paths.Where(File.Exists) )
			{
				using ( var openStream = File.OpenRead(path) )
				{
					var appSettings = await JsonSerializer.DeserializeAsync<AppSettings>(openStream);
					appSettingsList.Add(appSettings);
				}
			}

			if ( !appSettingsList.Any() ) return new AppSettings();

			var appSetting = appSettingsList.FirstOrDefault();
			
			for ( int i = 1; i < appSettingsList.Count; i++ )
			{
				var currentAppSetting = appSettingsList[i];
				AppSettingsCompareHelper.Compare(appSetting, currentAppSetting);
			}

			return appSetting;
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
				// defaults to appsettings.patch.json
				return builder;
			}

			builder.AddJsonFile(appSettingsPath, false, true);
			return builder;
		}
	}
}
