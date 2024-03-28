using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.externaldependencies;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.externaldependencies;

[TestClass]
public class ExternalDependenciesServiceTest
{
	private readonly ApplicationDbContext _dbContext;

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(ExternalDependenciesServiceTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	public ExternalDependenciesServiceTest()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	}

	[TestMethod]
	public async Task ExternalDependenciesServiceTest_ExifTool_Default()
	{
		var fakeExifToolDownload = new FakeExifToolDownload();
		var externalDependenciesService = new ExternalDependenciesService(fakeExifToolDownload,
			_dbContext, new FakeIWebLogger(), new AppSettings(), new FakeIGeoFileDownload());
		await externalDependenciesService.SetupAsync([]);

		Assert.AreEqual(new AppSettings().IsWindows, fakeExifToolDownload.Called[0]);
	}

	[DataTestMethod]
	[DataRow("osx-arm64", false)]
	[DataRow("linux-x64", false)]
	[DataRow("win-x64", true)]
	public async Task ExternalDependenciesServiceTest_ExifTool_DataTestMethod(string input, bool expected)
	{
		var fakeExifToolDownload = new FakeExifToolDownload();
		var externalDependenciesService = new ExternalDependenciesService(fakeExifToolDownload,
			_dbContext, new FakeIWebLogger(), new AppSettings(), new FakeIGeoFileDownload());
		
		await externalDependenciesService.SetupAsync(["--runtime", input]);

		Assert.AreEqual(expected, fakeExifToolDownload.Called[0]);
	}
}
