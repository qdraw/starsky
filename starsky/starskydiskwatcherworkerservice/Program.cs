using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.sync.WatcherServices;
using starsky.foundation.webtelemetry.Helpers;

namespace starskydiskwatcherworkerservice
{
	public class Program
	{
		private static IConfigurationRoot _configuration;
		private static AppSettings _appSettings;

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					_configuration = SetupAppSettings.AppSettingsToBuilder().Result;
					_appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, _configuration);
					_appSettings.ApplicationType = AppSettings.StarskyAppType
						.DiskWatcherWorkerService;
					services.AddMemoryCache();
					
					// Application Insights
					services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
					
					SetupLogging.AddLogging(services,_appSettings);

					var foundationDatabaseName = typeof(ApplicationDbContext).Assembly.FullName.Split(",").FirstOrDefault();
					new SetupDatabaseTypes(_appSettings,services, new ConsoleWrapper()).BuilderDb(foundationDatabaseName);
					
					new RegisterDependencies().Configure(services);
					services.AddHostedService<DiskWatcherBackgroundService>();
				});
	}
}
