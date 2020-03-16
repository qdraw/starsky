using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.storage.Helpers;
using starsky.Helpers;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytest.Controllers;
using starskytest.FakeMocks;
using Swashbuckle.AspNetCore.Swagger;

namespace starskytest.Helpers
{
	[TestClass]
	public class SwaggerHelperTest
	{
		private readonly AppSettings _appSettings;

		public SwaggerHelperTest()
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
			services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));

			// Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
			_appSettings = new AppSettings
			{
				Name = "starskySwaggerOutput",
				AddSwagger = true,
				AddSwaggerExport = true,
				TempFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
			};
		}
		
		[TestMethod]
		public async Task SwaggerTest_Integration_Test()
		{
			var swaggerFilePath = Path.Join(_appSettings.TempFolder, _appSettings.Name.ToLowerInvariant() + ".json");
			
			var storage = new FakeIStorage();
			var fakeSelectorStorage = new FakeSelectorStorage(storage);


			var host = WebHost.CreateDefaultBuilder()
				.UseUrls("http://localhost:5051")
				.ConfigureServices(services =>
				{
					services.AddMvcCore().AddApiExplorer();
					services.AddSwaggerGen();
					new SwaggerSetupHelper(_appSettings).Add01SwaggerGenHelper(services);

				})
				.Configure(app =>
				{

					app.UseRouting();
					app.UseEndpoints(endpoints =>
					{
						endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
					});

					new SwaggerSetupHelper(_appSettings).Add02AppUseSwaggerAndUi(app);
					using ( var serviceScope = app.ApplicationServices
						.GetRequiredService<IServiceScopeFactory>()
						.CreateScope() )
					{
						var swaggerProvider = ( ISwaggerProvider )serviceScope.ServiceProvider.GetService(typeof(ISwaggerProvider));
						new SwaggerExportHelper(null).Add03AppExport(_appSettings,fakeSelectorStorage, swaggerProvider);
					}

				}).Build();

			await host.StartAsync();
			await host.StopAsync();

			Assert.AreEqual(true,storage.ExistFile(swaggerFilePath));

			var swaggerFileContent =
				await new PlainTextFileHelper().StreamToStringAsync(
					storage.ReadStream(swaggerFilePath));

			System.Console.WriteLine("swaggerFileContent " + swaggerFileContent);

			Assert.AreEqual(true, swaggerFileContent.Contains($"\"Title\": \"{_appSettings.Name}\""));
		}

		
	}
}
