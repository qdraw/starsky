using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.root
{
	[TestClass]
	public class StartupTest
	{
		[TestMethod]
		public void Startup_ConfigureServices()
		{
			IServiceCollection serviceCollection = new ServiceCollection();
			// needed for: AddMetrics
			IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
			serviceCollection.AddSingleton(configuration); 
			
			// should not crash
			new Startup().ConfigureServices(serviceCollection);
			Assert.IsNotNull(serviceCollection);
		}
		
		[TestMethod]
		public void Startup_Configure()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddRouting();
			serviceCollection.AddSingleton<AppSettings, AppSettings>();
			IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
			serviceCollection.AddSingleton(configuration); 
			
			serviceCollection.AddAuthorization();
			serviceCollection.AddControllers();
			serviceCollection.AddLogging();
				
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };
			IHostApplicationLifetime lifetime = null;

			// should not crash
			new Startup().Configure(applicationBuilder, env, lifetime);
			
			Assert.IsNotNull(applicationBuilder);
			Assert.IsNotNull(env);
		}
				
		[TestMethod]
		public void Startup_ConfigureServicesConfigure1()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddRouting();
			serviceCollection.AddSingleton<AppSettings, AppSettings>();
			serviceCollection.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
			serviceCollection.AddSingleton<TelemetryConfiguration, TelemetryConfiguration>();
			IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
			serviceCollection.AddSingleton(configuration); 
			serviceCollection.AddAuthorization();
			serviceCollection.AddControllers();
			serviceCollection.AddLogging();
				
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };

			// should not crash
			var startup = new Startup();
			
			startup.ConfigureServices(serviceCollection);
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
			appSettings.ApplicationInsightsInstrumentationKey = "!";
			appSettings.UseRealtime = true;

			startup.Configure(applicationBuilder, env,null);
			
			Assert.IsNotNull(applicationBuilder);
			Assert.IsNotNull(env);
		}
	}
}
