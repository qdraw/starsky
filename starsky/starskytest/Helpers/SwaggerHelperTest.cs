using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytests.Controllers;

namespace starskytests.Helpers
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
			var swaggerFilePath = Path.Join(_appSettings.TempFolder, _appSettings.Name + ".json");

			Files.DeleteFile(swaggerFilePath);
			
			var host = WebHost.CreateDefaultBuilder()
				.UseUrls("http://localhost:5051")
				.ConfigureServices(services =>
				{
					services.AddMvcCore().AddApiExplorer(); // use core and AddApiExplorer to make it faster
					// https://offering.solutions/blog/articles/2017/02/07/difference-between-addmvc-addmvcore/
					services.AddSwaggerGen();
					new SwaggerHelper(_appSettings).Add01SwaggerGenHelper(services);

				})
				.Configure(app =>
				{
//					app.UseStaticFiles(); // disabled for now
					app.UseMvc();
					app.UseMvc(routes =>
					{
						routes.MapRoute(
							name: "default",
							template: "{controller=Home}/{action=Index}/{id?}");
					});
					new SwaggerHelper(_appSettings).Add02AppUseSwaggerAndUi(app); 
					new SwaggerHelper(_appSettings).Add03AppExport(app); 
				}).Build();

			await host.StartAsync();
			await host.StopAsync();

			Assert.AreEqual(true,Files.ExistFile(swaggerFilePath));

			var swaggerFileContent = new PlainTextFileHelper().ReadFile(swaggerFilePath);
				
			Assert.AreEqual(true, swaggerFileContent.Contains($"\"title\": \"{_appSettings.Name}\""));
			
			
		}

		
	}
}
