﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace starsky
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel(options =>
				{
					options.Limits.MaxRequestLineSize = 65536; //64Kb
					// AddServerHeader removes the header: Server: Kestrel
					options.AddServerHeader = false;
				})
				.UseStartup<Startup>();
	
	}
}
