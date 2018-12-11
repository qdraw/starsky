using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace starsky
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		// for swagger > public
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel(options =>
				{
					options.Limits.MaxRequestLineSize = 65536; //64Kb
				})
				.UseStartup<Startup>();
	
	}
}
