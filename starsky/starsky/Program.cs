using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starsky
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var appSettingsPath = Path.Join(
				new AppSettings().BaseDirectoryProject,
				"appsettings.json");
			await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(args,appSettingsPath);
			var builder = CreateWebHostBuilder(args);
			var startup = new Startup();
			startup.ConfigureServices(builder.Services);
			var app = builder.Build();
			var hostLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
			startup.Configure(app, builder.Environment, hostLifetime);
			await app.RunAsync();
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

			builder.Host.UseWindowsService();
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
