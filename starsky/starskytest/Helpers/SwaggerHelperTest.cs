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
using starsky.Helpers;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytest.Controllers;

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
			var swaggerFilePath = Path.Join(_appSettings.TempFolder, _appSettings.Name + ".yaml");

			FilesHelper.DeleteFile(swaggerFilePath);
			System.Console.WriteLine("swaggerFilePath " + swaggerFilePath);

			var host = WebHost.CreateDefaultBuilder()
				.UseUrls("http://localhost:5051")
				.ConfigureServices(services =>
				{

#if NETCOREAPP3_0
					services.AddMvcCore().AddApiExplorer();
						//.AddNewtonsoftJson();
#else
	services.AddMvcCore().AddApiExplorer(); // use core and AddApiExplorer to make it faster
		// https://offering.solutions/blog/articles/2017/02/07/difference-between-addmvc-addmvcore/
#endif

					services.AddSwaggerGen();
					new SwaggerHelper(_appSettings).Add01SwaggerGenHelper(services);

				})
				.Configure(app =>
				{

#if NETCOREAPP3_0
					app.UseRouting();
					app.UseEndpoints(endpoints =>
					{
						endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
					});
#else
	        app.UseMvc(routes =>
					 {
						 routes.MapRoute(
							 name: "default",
							 template: "{controller=Home}/{action=Index}/{id?}");
					 });
#endif
					new SwaggerHelper(_appSettings).Add02AppUseSwaggerAndUi(app);
					new SwaggerHelper(_appSettings).Add03AppExport(app);
				}).Build();

			await host.StartAsync();
			await host.StopAsync();

			Assert.AreEqual(true,FilesHelper.ExistFile(swaggerFilePath));

			var swaggerFileContent = new PlainTextFileHelper().ReadFile(swaggerFilePath);
				
			Assert.AreEqual(true, swaggerFileContent.Contains($"\"title\": \"{_appSettings.Name}\""));
			
			
		}

		
	}
}
