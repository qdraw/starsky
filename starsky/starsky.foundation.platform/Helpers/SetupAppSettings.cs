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
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.platform.Helpers
{
	public static class SetupAppSettings
	{
		public static async Task<ServiceCollection> FirstStepToAddSingleton(ServiceCollection services)
		{
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			var configurationRoot = await AppSettingsToBuilder();
			services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));
			return services;
		}
		
		/// <summary>
		/// Default appSettings.json to builder
		/// </summary>
		/// <returns>ConfigBuilder</returns>
		public static async Task<IConfigurationRoot> AppSettingsToBuilder()
		{
			var appSettings = new AppSettings();
			var builder = new ConfigurationBuilder();

			var settings = await MergeJsonFiles(appSettings.BaseDirectoryProject);
			
			// Make sure is wrapped in a AppContainer app
			var utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(settings);

			builder
				.AddJsonStream(new MemoryStream(utf8Bytes))
				// overwrite envs
				// current dir gives problems on linux arm
				.AddEnvironmentVariables();
				
			return builder.Build();
		}

		internal static string AppSettingsMachineNameWithDot()
		{
			// to remove spaces and other signs, check help to get your name
			return $"appsettings.{Environment.MachineName.ToLowerInvariant()}."; // dot here
		}

		private static List<string> Order(string baseDirectoryProject)
		{
			var appSettingsMachine = AppSettingsMachineNameWithDot();
			return new List<string>
			{
				Path.Combine(baseDirectoryProject, "appsettings.json"),
				Path.Combine(baseDirectoryProject, "appsettings.default.json"),
				Path.Combine(baseDirectoryProject, "appsettings.patch.json"),
				Path.Combine(baseDirectoryProject, appSettingsMachine + "json"),
				Path.Combine(baseDirectoryProject, appSettingsMachine + "patch.json"),
				Environment.GetEnvironmentVariable("app__appsettingspath")
			};
		}

		internal static async Task<AppContainerAppSettings> MergeJsonFiles(string baseDirectoryProject)
		{
			var paths = Order(baseDirectoryProject);
			var appSettingsList = new List<AppContainerAppSettings>();

			foreach ( var path in paths.Where(File.Exists) )
			{
				var appSettings = await Read(path);
				appSettingsList.Add(appSettings);
			}

			if ( !appSettingsList.Any() ) return new AppContainerAppSettings{App = new AppSettings()};
			var appSetting = appSettingsList.FirstOrDefault()!;
			
			for ( var i = 1; i < appSettingsList.Count; i++ )
			{
				var currentAppSetting = appSettingsList[i];
				AppSettingsCompareHelper.Compare(appSetting.App, currentAppSetting.App);
			}

			return appSetting;
		}

		internal static async Task<AppContainerAppSettings> Read(string path)
		{
			if ( !File.Exists(path) )
			{
				return new AppContainerAppSettings();
			}
			
			using ( var openStream = File.OpenRead(path) )
			{
				return  await JsonSerializer
					.DeserializeAsync<AppContainerAppSettings>(openStream,
						new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true,
							Converters =
							{
								new JsonBoolQuotedConverter(),
							},
						});
			}
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
			var serviceProvider = services.BuildServiceProvider();
            
			return serviceProvider.GetRequiredService<AppSettings>();
		}

	}
}
