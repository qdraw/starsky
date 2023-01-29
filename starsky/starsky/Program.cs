using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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

		private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureKestrel((_, options) =>
				{
					// instead of UseKestrel @see: https://stackoverflow.com/a/55926191
					options.Limits.MaxRequestLineSize = 65536; //64Kb
					// AddServerHeader removes the header: Server: Kestrel
					options.AddServerHeader = false;
				})
				.UseStartup<Startup>();
	}
}
