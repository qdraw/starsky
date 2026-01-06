using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;
using starsky.foundation.import.Services;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode.Model;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class ObjectCreateIndexItemServiceTests
{
	[TestMethod]
	public async Task ShouldIgnoreReverseGeoCode()
	{
		var sut = new ObjectCreateIndexItemService(new AppSettings(), null!);
		var result = await sut.TransformCreateIndexItem(
			new ImportIndexItem { FileIndexItem = new FileIndexItem(), Status = ImportStatus.Ok },
			new ImportSettingsModel { ReverseGeoCode = false });

		Assert.IsNotNull(result);
		Assert.AreEqual(ImportStatus.Ok, result.Status);
	}

	[TestMethod]
	public async Task ShouldHitReverseGeoCode()
	{
		var sut = new ObjectCreateIndexItemService(new AppSettings(),
			new FakeIReverseGeoCodeService(new GeoLocationModel
			{
				LocationState = "test state",
				LocationCity = "test city",
				LocationCountry = "test country",
				LocationCountryCode = "TC"
			}));
		var result = await sut.TransformCreateIndexItem(
			new ImportIndexItem
			{
				FileIndexItem = new FileIndexItem { Latitude = 52, Longitude = 4 },
				Status = ImportStatus.Ok
			}, new ImportSettingsModel { ReverseGeoCode = true });

		// ReverseGeoCodeService should set the city and country
		Assert.IsNotNull(result);
		Assert.AreEqual(ImportStatus.Ok, result.Status);
		Assert.AreEqual("test city", result.FileIndexItem?.LocationCity);
		Assert.AreEqual("test country", result.FileIndexItem?.LocationCountry);
	}
	
	[TestMethod]
	public async Task ShouldFailReverseGeoCode()
	{
		var sut = new ObjectCreateIndexItemService(new AppSettings(),
			new FakeIReverseGeoCodeService(new GeoLocationModel()));
		var result = await sut.TransformCreateIndexItem(
			new ImportIndexItem
			{
				FileIndexItem = new FileIndexItem { Latitude = 52, Longitude = 4 },
				Status = ImportStatus.Ok
			}, new ImportSettingsModel { ReverseGeoCode = true });

		// ReverseGeoCodeService should set the city and country
		Assert.IsNotNull(result);
		Assert.AreEqual(ImportStatus.Ok, result.Status);
		Assert.AreEqual(string.Empty, result.FileIndexItem?.LocationCity);
		Assert.AreEqual(string.Empty, result.FileIndexItem?.LocationCountry);
	}
}
