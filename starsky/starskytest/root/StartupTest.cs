using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.foundation.platform.Models;

namespace starskytest.root
{
	[TestClass]
	public class StartupTest
	{
		[TestMethod]
		public void Startup_ConfigureServices()
		{
			IServiceCollection serviceCollection = new ServiceCollection();

			// should not crash
			new Startup().ConfigureServices(serviceCollection);
		}
		
		[TestMethod]
		public void Startup_Configure()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddRouting();
			serviceCollection.AddSingleton<AppSettings, AppSettings>();
			serviceCollection.AddAuthorization();
			serviceCollection.AddControllers();
			serviceCollection.AddLogging();
				
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };
			
			// should not crash
			new Startup().Configure(applicationBuilder, env);
		}
	}
}
