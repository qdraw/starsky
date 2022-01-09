using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.Helpers;
using starskytest.FakeMocks;
using Swashbuckle.AspNetCore.Swagger;

namespace starskytest.Helpers
{
	public class FakeISwaggerProvider : ISwaggerProvider
	{
		public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
		{
			return new OpenApiDocument{ Components = new OpenApiComponents{ Links = null}};
		}
	}
	
	[TestClass]
	public class SwaggerExportHelperTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ServiceProvider _serviceProvider;

		public SwaggerExportHelperTest()
		{
			var services = new ServiceCollection();
			var dict = new Dictionary<string, string>
			{
				{ "App:Verbose", "true" },
				{ "App:AddSwagger", "true" },
				{ "App:AddSwaggerExport", "true" },
				{ "App:TempFolder", Path.DirectorySeparatorChar.ToString() },
				{ "App:Name", "starsky_test_name" },
			};
			// Start using dependency injection
			var builder = new ConfigurationBuilder();  
			// Add random config to dependency injection
			builder.AddInMemoryCollection(dict);
			// build config
			var configuration = builder.Build();
			// inject config as object to a service
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			services.AddScoped<ISwaggerProvider, FakeISwaggerProvider>();
			services.AddScoped<IHostApplicationLifetime, FakeIApplicationLifetime>();

			_serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public void GenerateSwagger_NullInput()
		{
			Assert.AreEqual(string.Empty, SwaggerExportHelper.GenerateSwagger(null, null));
		}
		
		[TestMethod]
		public void Add03AppExport_disabled_AddSwagger()
		{
			var appSettings = new AppSettings {AddSwagger = true, AddSwaggerExport = false};
			var swagger = new SwaggerExportHelper(null, new FakeIWebLogger());
			var result = swagger.Add03AppExport(appSettings, new FakeSelectorStorage(), null);
			
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void Add04SwaggerExportExitAfter_True()
		{
			var appSettings = new AppSettings {AddSwagger = true, AddSwaggerExport = true, AddSwaggerExportExitAfter = true};
			var swagger = new SwaggerExportHelper(null, new FakeIWebLogger());
			var appLifeTime = new FakeIApplicationLifetime();
			var result = swagger.Add04SwaggerExportExitAfter(appSettings, appLifeTime);
			
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void Add04SwaggerExportExitAfter_False()
		{
			var appSettings = new AppSettings {AddSwagger = true, AddSwaggerExport = false, AddSwaggerExportExitAfter = false};
			var swagger = new SwaggerExportHelper(null, new FakeIWebLogger());
			var appLifeTime = new FakeIApplicationLifetime();
			var result = swagger.Add04SwaggerExportExitAfter(appSettings, appLifeTime);
			
			Assert.IsFalse(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Add03AppExport_null_ArgumentException()
		{
			var appSettings = new AppSettings {AddSwagger = true, AddSwaggerExport = true};
			var swagger = new SwaggerExportHelper(null, new FakeIWebLogger());
			swagger.Add03AppExport(appSettings, new FakeSelectorStorage(), null);
		}
		
		[TestMethod]
		public void Add03AppExport_disabled_AddSwaggerExport()
		{
			var appSettings = new AppSettings {AddSwagger = false, AddSwaggerExport = true};
			var swagger = new SwaggerExportHelper(null, new FakeIWebLogger());
			var result = swagger.Add03AppExport(appSettings, new FakeSelectorStorage(), null);
			
			Assert.IsFalse(result);
		}


		[TestMethod]
		public void ExecuteAsync_ShouldWrite()
		{
			new SwaggerExportHelper(_serviceScopeFactory).ExecuteAsync();
			var selectorStorage = _serviceProvider.GetService<ISelectorStorage>();
			var storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

			Assert.IsTrue(storage.ExistFile( Path.DirectorySeparatorChar +"starsky_test_name.json"));
		}
	}
}
