using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.Helpers;
using starskytest.Controllers;
using starskytest.FakeMocks;

namespace starskytest.Helpers;

[TestClass]
public sealed class OpenApiHelperTest
{
	private readonly AppSettings _appSettings;

	public OpenApiHelperTest()
	{
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(ExportControllerTest));

		var services = new ServiceCollection();

		// Inject Config helper
		services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

		// Start using dependency injection
		var builder = new ConfigurationBuilder();
		// build config
		var configuration = builder.Build();
		// inject config as object to a service
		services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));

		// Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
		_appSettings = new AppSettings
		{
			Name = "starskyOpenApiOutput",
			AddOpenApi = true,
			AddOpenApiExport = true,
			AddScalar = true,
			TempFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!
		};
	}

	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task OpenApiTest_Integration_Test()
	{
		var openApiFilePath = Path.Join(_appSettings.TempFolder,
			_appSettings.Name.ToLowerInvariant() + ".json");

		var storage = new FakeIStorage();
		var fakeSelectorStorage = new FakeSelectorStorage(storage);


		var hostBuilder = WebApplication.CreateBuilder();
		hostBuilder.WebHost.UseUrls("http://localhost:5051");
		// the starsky controllers cover the ApiExplorer visibility convention
		hostBuilder.Services.AddMvcCore().AddApiExplorer()
			.AddApplicationPart(typeof(OpenApiSetupHelper).Assembly);
		new OpenApiSetupHelper(_appSettings).Add01OpenApiHelper(hostBuilder.Services);
		var app = hostBuilder.Build();

		app.UseRouting();
		app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
		new OpenApiSetupHelper(_appSettings).Add02AppUseOpenApiAndScalar(app);

		using ( var serviceScope = app.Services
			       .GetRequiredService<IServiceScopeFactory>()
			       .CreateScope() )
		{
			var documentProvider = serviceScope.ServiceProvider
				.GetRequiredKeyedService<IOpenApiDocumentProvider>(_appSettings.Name.ToLowerInvariant());
			await new OpenApiExportHelper(null!).Add03AppExport(_appSettings,
				fakeSelectorStorage, documentProvider);
		}

		await app.StartAsync(TestContext.CancellationTokenSource.Token);
		await app.StopAsync(TestContext.CancellationTokenSource.Token);

		Assert.IsTrue(storage.ExistFile(openApiFilePath));

		var openApiFileContent =
			await StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(openApiFilePath));

		Console.WriteLine("openApiFileContent " + openApiFileContent);

		Assert.Contains($"\"title\": \"{_appSettings.Name}\"", openApiFileContent);
		Assert.Contains("\"scheme\": \"basic\"", openApiFileContent);
		// attribute routed action on a controller that does not use [ApiController]
		Assert.Contains("\"/api/account/status\"", openApiFileContent);
		// opt-out with IgnoreApi
		Assert.DoesNotContain("MetricsDebug", openApiFileContent);
	}

	[TestMethod]
	public async Task AddScalar_ImpliesAddOpenApi()
	{
		var appSettings = new AppSettings { Name = "starskyscalaronly", AddScalar = true };

		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddMvcCore().AddApiExplorer()
			.AddApplicationPart(typeof(OpenApiSetupHelper).Assembly);
		new OpenApiSetupHelper(appSettings).Add01OpenApiHelper(builder.Services);
		var app = builder.Build();
		app.UseRouting();
		new OpenApiSetupHelper(appSettings).Add02AppUseOpenApiAndScalar(app);

		await app.StartAsync(TestContext.CancellationTokenSource.Token);
		var client = app.GetTestClient();
		var response = await client.GetAsync("/openapi/starskyscalaronly.json",
			TestContext.CancellationTokenSource.Token);
		// controllers (with [Authorize]) must not run in the docs endpoint block
		var controllerResponse = await client.GetAsync("/api/account/status",
			TestContext.CancellationTokenSource.Token);
		await app.StopAsync(TestContext.CancellationTokenSource.Token);

		Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
		Assert.AreEqual(System.Net.HttpStatusCode.NotFound, controllerResponse.StatusCode);
	}
}
