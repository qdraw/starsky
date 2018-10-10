using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace starsky
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}
		
		private static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel(options =>
				{
					options.Limits.MaxRequestHeaderCount = 20;
					options.Limits.MaxRequestLineSize = 65536; //64Kb
				})
				.UseStartup<Startup>()
				.Build();
	}
}
