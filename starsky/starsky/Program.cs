using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starsky
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(args,
				Path.Join(new AppSettings().BaseDirectoryProject,
					"appsettings.json"));
			await CreateWebHostBuilder(args).Build().RunAsync();
		}

		private static IHostBuilder CreateWebHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseWindowsService()
				.ConfigureWebHost(options =>
				{
					options.UseKestrel();
					options.ConfigureKestrel(k =>
					{
						// instead of UseKestrel @see: https://stackoverflow.com/a/55926191
						k.Limits.MaxRequestLineSize = 65536; //64Kb
						// AddServerHeader removes the header: Server: Kestrel
						k.AddServerHeader = false;
					});
					
					// Configure IIS for Windows
					options.UseIIS();
					options.UseIISIntegration();
					
					options.UseStartup<Startup>();
				});
	}
}
