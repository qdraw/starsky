using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.Helpers;
using starskytest.FakeMocks;

namespace starskytest.Helpers;

public sealed class FakeIOpenApiDocumentProvider : IOpenApiDocumentProvider
{
	public Task<OpenApiDocument> GetOpenApiDocumentAsync(
		CancellationToken cancellationToken = default)
	{
		return Task.FromResult(new OpenApiDocument
		{
			Components = new OpenApiComponents { Links = null }
		});
	}
}

[TestClass]
public sealed class OpenApiExportHelperTest
{
	private readonly ServiceProvider _serviceProvider;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public OpenApiExportHelperTest()
	{
		var services = new ServiceCollection();
		var dict = new Dictionary<string, string?>
		{
			{ "App:Verbose", "true" },
			{ "App:AddOpenApi", "true" },
			{ "App:AddOpenApiExport", "true" },
			{ "App:TempFolder", Path.DirectorySeparatorChar.ToString() },
			{ "App:Name", "starsky_test_name" }
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
		services.AddKeyedScoped<IOpenApiDocumentProvider, FakeIOpenApiDocumentProvider>(
			"starsky_test_name");
		services.AddScoped<IHostApplicationLifetime, FakeIApplicationLifetime>();

		_serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task GenerateOpenApi_NullInput()
	{
		Assert.AreEqual(string.Empty, await OpenApiExportHelper.GenerateOpenApi(null));
	}

	[TestMethod]
	public async Task Add03AppExport_disabled_AddOpenApi()
	{
		var appSettings = new AppSettings { AddOpenApi = true, AddOpenApiExport = false };
		var export = new OpenApiExportHelper(null!, new FakeIWebLogger());
		var result = await export.Add03AppExport(appSettings, new FakeSelectorStorage(), null);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Add04OpenApiExportExitAfter_True()
	{
		var appSettings = new AppSettings
		{
			AddOpenApi = true, AddOpenApiExport = true, AddOpenApiExportExitAfter = true
		};
		var export = new OpenApiExportHelper(null!, new FakeIWebLogger());
		var appLifeTime = new FakeIApplicationLifetime();
		var result = export.Add04OpenApiExportExitAfter(appSettings, appLifeTime);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Add04OpenApiExportExitAfter_False()
	{
		var appSettings = new AppSettings
		{
			AddOpenApi = true, AddOpenApiExport = false, AddOpenApiExportExitAfter = false
		};
		var export = new OpenApiExportHelper(null!, new FakeIWebLogger());
		var appLifeTime = new FakeIApplicationLifetime();
		var result = export.Add04OpenApiExportExitAfter(appSettings, appLifeTime);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Add03AppExport_null_ArgumentException()
	{
		var appSettings = new AppSettings { AddOpenApi = true, AddOpenApiExport = true };
		var export = new OpenApiExportHelper(null!, new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			export.Add03AppExport(appSettings, new FakeSelectorStorage(), null));
	}

	[TestMethod]
	public async Task Add03AppExport_disabled_AddOpenApiExport()
	{
		var appSettings = new AppSettings { AddOpenApi = false, AddOpenApiExport = true };
		var export = new OpenApiExportHelper(null!, new FakeIWebLogger());
		var result = await export.Add03AppExport(appSettings, new FakeSelectorStorage(), null);

		Assert.IsFalse(result);
	}


	[TestMethod]
	[SuppressMessage("Usage", "MSTEST0049:Flow TestContext.CancellationToken to async operations")]
	public async Task ExecuteAsync_ShouldWrite()
	{
		await new OpenApiExportHelper(_serviceScopeFactory).ExecuteAsync();
		var selectorStorage = _serviceProvider.GetRequiredService<ISelectorStorage>();
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

		Assert.IsTrue(storage.ExistFile(Path.DirectorySeparatorChar + "starsky_test_name.json"));
	}

	public TestContext TestContext { get; set; } = null!;
}
