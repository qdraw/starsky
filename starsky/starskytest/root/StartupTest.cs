using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.root;

[TestClass]
public sealed class StartupTest
{
	[TestMethod]
	public void Startup_ConfigureServices()
	{
		IServiceCollection serviceCollection = new ServiceCollection();
		// needed for: AddMetrics
		IConfiguration configuration =
			new ConfigurationRoot(new List<IConfigurationProvider>());
		serviceCollection.AddSingleton(configuration);

		new Startup().ConfigureServices(serviceCollection);

		var configurationService =
			serviceCollection.BuildServiceProvider().GetService<IConfiguration>();
		Assert.IsNotNull(configurationService, "IConfiguration should be registered.");
	}

	[TestMethod]
	public void Startup_ConfigureServicesConfigure()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddRouting();
		serviceCollection.AddSingleton<AppSettings, AppSettings>();
		serviceCollection.AddSingleton<IWebLogger, FakeIWebLogger>();
		serviceCollection
			.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
		IConfiguration configuration =
			new ConfigurationRoot(new List<IConfigurationProvider>());
		serviceCollection.AddSingleton(configuration);
		serviceCollection.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();
		serviceCollection.AddAuthorization();
		serviceCollection.AddControllers();
		serviceCollection.AddLogging();
		serviceCollection.AddMvcCore().AddApiExplorer().AddAuthorization().AddViews();

		var serviceProvider = serviceCollection.BuildServiceProvider();
		var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();

		var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
		IHostEnvironment env =
			new HostingEnvironment { EnvironmentName = Environments.Development };

		// should not crash
		var startup = new Startup();

		startup.ConfigureServices(serviceCollection);
		var appSettings = serviceProvider.GetRequiredService<AppSettings>();
		appSettings.UseRealtime = true;

		startup.Configure(applicationBuilder, env);

		Assert.IsNotNull(applicationBuilder);
	}

	[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Local")]
	private static List<object?>? GetMiddlewareInstance(IApplicationBuilder app)
	{
		const string middlewareTypeName =
			"Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware";
		var appBuilderType = typeof(ApplicationBuilder);
		const BindingFlags bindingTypes1 = BindingFlags.Instance |
		                                   BindingFlags.NonPublic;
		var middlewareField = appBuilderType.GetField("_components", bindingTypes1);
		var components = middlewareField?.GetValue(app);

		if ( components != null )
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
				const BindingFlags bindingTypes = BindingFlags.Instance |
				                                  BindingFlags.NonPublic |
				                                  BindingFlags.Public;
				var privatePropertyInfo = type?.GetField("_args", bindingTypes);
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

		Assert.IsTrue(result.Item1);
		Assert.IsFalse(result.Item2);
		Assert.IsFalse(result.Item3);

		var middlewareInstance =
			GetMiddlewareInstance(applicationBuilder)?.FirstOrDefault() as object?[];
		var value = middlewareInstance?.FirstOrDefault() as OptionsWrapper<StaticFileOptions>;

		Assert.IsFalse(value?.Value.RequestPath.HasValue);
		Assert.AreEqual(string.Empty, value?.Value.RequestPath.Value);
	}

	[TestMethod]
	public void BasicFlow_Assets()
	{
		var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject,
			"wwwroot"));
		storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject,
			"clientapp", "build", "assets"));

		var startup = new Startup();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<AppSettings, AppSettings>();
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();

		var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
		startup.ConfigureServices(serviceCollection);
		var result = startup.SetupStaticFiles(applicationBuilder);

		Console.WriteLine("result:");
		Console.WriteLine("1: " + result.Item1 + " 2: " + result.Item2 + " 3: " + result.Item3);

		Assert.IsTrue(result.Item1);
		Assert.IsTrue(result.Item2);
		Assert.IsTrue(result.Item3);

		var middlewareInstance =
			GetMiddlewareInstance(applicationBuilder)?.ToList()[1] as object?[];
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

		var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject,
			"wwwroot"));
		storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject,
			"clientapp", "build", "assets"));

		var result = startup.SetupStaticFiles(applicationBuilder);

		Console.WriteLine("result:");
		Console.WriteLine("1: " + result.Item1 + " 2: " + result.Item2 + " 3: " + result.Item3);

		Assert.IsTrue(result.Item1);
		Assert.IsTrue(result.Item2);
		Assert.IsTrue(result.Item3);

		var middlewareInstance =
			GetMiddlewareInstance(applicationBuilder)?.ToList()[2] as object?[];
		var value = middlewareInstance?.FirstOrDefault() as OptionsWrapper<StaticFileOptions>;

		Assert.IsTrue(value?.Value.RequestPath.HasValue);
		Assert.AreEqual("/assets", value?.Value.RequestPath.Value);
	}

	[TestMethod]
	public void BasicFlow_Assets_NotFound()
	{
		var startup = new Startup();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<AppSettings, AppSettings>();
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var serviceProviderInterface = serviceProvider.GetRequiredService<IServiceProvider>();

		var applicationBuilder = new ApplicationBuilder(serviceProviderInterface);
		startup.ConfigureServices(serviceCollection);

		var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		storage.CreateDirectory(Path.Combine(new AppSettings().BaseDirectoryProject,
			"wwwroot"));

		var result = startup.SetupStaticFiles(applicationBuilder, "not-found-folder-name");

		Console.WriteLine("result:");
		Console.WriteLine("1: " + result.Item1 + " 2: " + result.Item2 + " 3: " + result.Item3);

		Assert.IsTrue(result.Item1);
		Assert.IsTrue(result.Item2);
		Assert.IsFalse(result.Item3);
	}

	[TestMethod]
	public void PrepareResponse_CheckValues()
	{
		var httpContext = new DefaultHttpContext();
		var context = new StaticFileResponseContext(httpContext,
			new NotFoundFileInfo("test"));

		// Act
		Startup.PrepareResponse(context);

		// Assert
		Assert.IsNotEmpty(context.Context.Response.Headers.Expires.ToString());
		Assert.AreEqual("public, max-age=31536000",
			context.Context.Response.Headers.CacheControl.ToString());
	}
}
