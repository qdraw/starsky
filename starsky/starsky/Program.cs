using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.project.web.Helpers;

namespace starsky
{
	public static class Program
	{
		[SuppressMessage("Usage", "S6603: The collection-specific TrueForAll " +
		                          "method should be used instead of the All extension")]
		public static async Task Main(string[] args)
		{
			var appSettingsPath = Path.Join(
				new AppSettings().BaseDirectoryProject,
				"appsettings.json");
			await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(args, appSettingsPath);

			var builder = CreateWebHostBuilder(args);
			var startup = new Startup(args);
			startup.ConfigureServices(builder.Services);
			builder.Host.UseWindowsService();

			var app = builder.Build();
			startup.Configure(app, builder.Environment);

			await RunAsync(app, args.All(p => p != "--do-not-start"));
		}

		internal static async Task<bool> RunAsync(WebApplication webApplication,
			bool startImmediately = true)
		{
			if ( !startImmediately )
			{
				return false;
			}

			try
			{
				await webApplication.RunAsync();
			}
			catch ( TaskCanceledException )
			{
				return false;
			}

			return true;
		}

		private static WebApplicationBuilder CreateWebHostBuilder(string[] args)
		{
			var settings = new WebApplicationOptions
			{
				Args = args,
				//set ContentRootPath so that builder.Host.UseWindowsService() doesn't crash when running as a service
				ContentRootPath = AppContext.BaseDirectory,
			};
			var builder = WebApplication.CreateBuilder(settings);

			builder.WebHost.ConfigureKestrel(k =>
			{
				k.Limits.MaxRequestLineSize = 65536; //64Kb
				// AddServerHeader removes the header: Server: Kestrel
				k.AddServerHeader = false;
			});

			builder.WebHost.UseIIS();
			builder.WebHost.UseIISIntegration();

			return builder;
		}
	}
}
