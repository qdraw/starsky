using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.root
{
	[TestClass]
	public sealed class StartupTest
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
			serviceCollection.AddMvcCore().AddApiExplorer().AddAuthorization().AddViews();

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };

			// should not crash
			var startup = new Startup();
			
			startup.ConfigureServices(serviceCollection);
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
			appSettings.ApplicationInsightsConnectionString = "!";
			appSettings.UseRealtime = true;

			startup.Configure(applicationBuilder, env, new FakeIApplicationLifetime());
			
			Assert.IsNotNull(applicationBuilder);
			Assert.IsNotNull(env);
		}

		private static IEnumerable<object?>? GetMiddlewareInstance(IApplicationBuilder app)
		{
			const string middlewareTypeName = "Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware";
			var appBuilderType = typeof(ApplicationBuilder);
			var middlewareField = appBuilderType.GetField("_components", BindingFlags.Instance | BindingFlags.NonPublic);
			var components = middlewareField?.GetValue(app);

			if (components != null)
			{
				var element = components as List<Func<RequestDelegate, RequestDelegate>>;

				var middlewares = element?.Where(p =>
					p.Target?.ToString() == middlewareTypeName);
				if ( middlewares == null )
				{
					return null;
				}

				var status = new List<object?>();
				foreach ( var middleware in middlewares )
				{

					var type = middleware.Target?.GetType();
					var privatePropertyInfo = type?.GetField("_args", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					var privateFieldValue =
						privatePropertyInfo?.GetValue(middleware.Target) as object[];
					
					status.Add(privateFieldValue);
				}
				return status;
			}

			return null;
		}
		
		[TestMethod]
		public void BasicFlow_Default()
		{
			var startup = new Startup();
			var serviceCollection = new ServiceCollection();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			var result = startup.SetupStaticFiles(applicationBuilder);
			
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Item1);
			Assert.IsFalse(result.Item2);
			Assert.IsFalse(result.Item3);
			
			var middlewareInstance = GetMiddlewareInstance(applicationBuilder)?.FirstOrDefault() as object?[];
			var value = middlewareInstance?.FirstOrDefault() as OptionsWrapper<StaticFileOptions>;
			
			Assert.IsFalse(value?.Value.RequestPath.HasValue);
			Assert.AreEqual(string.Empty, value?.Value.RequestPath.Value);
		}
		
		[TestMethod]
		public void BasicFlow_Assets()
		{
			var startup = new Startup();
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton<AppSettings, AppSettings>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var storage = new StorageHostFullPathFilesystem();
			storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject, "wwwroot"));
			storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject, "clientapp", "build"));

			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			startup.ConfigureServices(serviceCollection);
			var result = startup.SetupStaticFiles(applicationBuilder);

			Assert.IsNotNull(result);
			
			Assert.IsTrue(result.Item1);
			Assert.IsTrue(result.Item2);
			Assert.IsTrue(result.Item3);

			var middlewareInstance = GetMiddlewareInstance(applicationBuilder)?.ToList()[1] as object?[];
			var value = middlewareInstance?.FirstOrDefault() as OptionsWrapper<StaticFileOptions>;
			
			Assert.IsFalse(value?.Value.RequestPath.HasValue);
			Assert.AreEqual(string.Empty, value?.Value.RequestPath.Value);
		}
		
		[TestMethod]
		public void BasicFlow_Assets2()
		{
			var startup = new Startup();
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton<AppSettings, AppSettings>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();
			
			var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
			startup.ConfigureServices(serviceCollection);
			
			var storage = new StorageHostFullPathFilesystem();
			storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject, "wwwroot"));
			storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject, "clientapp", "build"));

			var result = startup.SetupStaticFiles(applicationBuilder);
			Assert.IsNotNull(result);
			
			Assert.IsTrue(result.Item1);
			Assert.IsTrue(result.Item2);
			Assert.IsTrue(result.Item3);

			var middlewareInstance = GetMiddlewareInstance(applicationBuilder)?.ToList()[2] as object?[];
			var value = middlewareInstance?.FirstOrDefault() as OptionsWrapper<StaticFileOptions>;
			
			Assert.IsTrue(value?.Value.RequestPath.HasValue);
			Assert.AreEqual("/assets", value?.Value.RequestPath.Value);
		}
	}
}
