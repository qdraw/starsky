using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.GeoNamesCities;
using starsky.foundation.database.Models;
using starsky.foundation.geo.GeoNameCitySeed;
using starsky.foundation.geo.LocationNameSearch;
using starsky.foundation.geo.LocationNameSearch.Interfaces;
using starsky.foundation.geo.LocationNameSearch.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.Controllers;

public class FakeLocationNameService : ILocationNameService
{
	public Task<List<CityTimezoneResult>> SearchCityTimezone(string dateTime, string cityName)
	{
		if ( cityName == "Amsterdam" && dateTime == "2026-01-30T12:00:00Z" )
		{
			return Task.FromResult(new List<CityTimezoneResult>
			{
				new()
				{
					Id = "Europe/Amsterdam",
					DisplayName = "Central European Time",
					AltText = "+01:00"
				}
			});
		}

		return Task.FromResult(new List<CityTimezoneResult>());
	}

	public Task<List<GeoNameCity>> SearchCity(string cityName)
	{
		return Task.FromResult(new List<GeoNameCity>());
	}
}

[TestClass]
public class GeoLocationNameControllerTest : VerifyBase
{
	[TestMethod]
	public async Task SearchCityTimezone_ReturnsOk()
	{
		// Arrange
		var fakeService = new FakeLocationNameService();
		var controller = new GeoLocationNameController(fakeService);

		// Act
		var result = await controller.SearchCityTimezone("2026-01-30T12:00:00Z", "Amsterdam");

		// Assert
		var okResult = result as OkObjectResult;
		Assert.IsNotNull(okResult);
		Assert.AreEqual(200, okResult.StatusCode ?? 200);
		Assert.IsNotNull(okResult.Value);
	}

	[TestMethod]
	public async Task SearchCity_ReturnsOk_Verify()
	{
		// Arrange
		await using var dbContext = CreateDbContext();
		var (controller, fakeGeoFileDownload) = await CreateController(dbContext);

		// Act
		var result = await controller.SearchCity("Amsterdam");

		// Assert
		var okResult = result as OkObjectResult;
		Assert.IsNotNull(okResult);
		Assert.AreEqual(200, okResult.StatusCode ?? 200);
		Assert.IsNotNull(okResult.Value);

		var cityTimezoneResults = okResult.Value as List<GeoNameCity>;
		Assert.IsNotNull(cityTimezoneResults);
		Assert.HasCount(1, cityTimezoneResults);
		Assert.AreEqual("Europe/Amsterdam", cityTimezoneResults[0].TimeZoneId);

		Assert.AreEqual(1, fakeGeoFileDownload.Count);
		await Verify(cityTimezoneResults);
	}

	private static ApplicationDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("GeoLocationNameControllerTestDb")
			.Options;

		return new ApplicationDbContext(options);
	}

	private static async Task<(GeoLocationNameController, FakeIGeoFileDownload)> CreateController(
		ApplicationDbContext dbContext)
	{
		if ( !await dbContext.GeoNameCities.AnyAsync(p => p.GeonameId == 2759794) )
		{
			dbContext.GeoNameCities.Add(new GeoNameCity
			{
				GeonameId = 2759794,
				Name = "Amsterdam",
				AsciiName = "Amsterdam",
				AlternateNames =
					"AMS,Aemstelredamme,Aemsterdam,Amestelledamme,Amesterda,Amesterdam," +
					"Amesterdao,Amesterdã,Amesterdão,Amistardam,Amseutereudam,Amstadem," +
					"Amstardam,Amstardām,Amstedam,Amstehrdam,Amsteladamum,Amstelodamum," +
					"Amstelodhamon,Amstelodhámon,Amsterda,Amsterdam,Amsterdama," +
					"Amsterdamas,Amsterdame,Amsterdami,Amsterdamo,Amsterdams,Amsterdamu," +
					"Amsterdan,Amsterdã,Amsterntam,Amsterodam,Amstyerdam,Amstèdam," +
					"Amstèrdame,Amstérdam,Amstɛrɩdam,Amstʻertam,Amsut'erudam,Amszterdam," +
					"Amsŭt'erŭdam,Amusitedan,Amusuterudamu,Damsko,I-Amsterdami,Mokum," +
					"Mokum Aleph,a mu si te dan,aimstardaima,amasataradama,amastaradama," +
					"amastararyama,amseuteleudam,amstardama,amstartam,amstrdam," +
					"amusuterudamu,anstardyam,emstaradyama,xamstexrdam,yەmstەrdam," +
					"Àmsterdam,Ámsterdam,Ámsterdan,Âmesterdâm,Āmǔsītèdān,Άμστερνταμ," +
					"Амстердам,Амстэрдам,Ամսթերտամ,Ամստերդամ,אמסטערדאם,אמסטרדם," +
					"آمستردام,أمستردام,ئامستېردام,ئەمستەردام,امستردام,امسټرډام," +
					"ایمسٹرڈیم,ܐܡܣܛܪܕܐܡ,अ‍ॅम्स्टरडॅम,आम्स्टर्डम,एम्स्तरद्याम," +
					"ऐम्स्टर्डैम,আমস্টারডাম,ਅਮਸਤਰਦਮ,ଆମଷ୍ଟରଡ଼୍ୟାମ,ஆம்ஸ்டர்டம்," +
					"ಆಂಸ್ಟರ್ಡ್ಯಾಮ್,ആംസ്റ്റർഡാം,ඈම්ස්ටර්ඩෑම්,อัมสเตอร์ดัม," +
					"ཨེམ་སི་ཊར་ཌམ།,အမ်စတာဒမ်မြို့,ამსტერდამი,አምስተርዳም," +
					"アムステルダム,阿姆斯特丹,암스테르담",
				Latitude = 52.37403,
				Longitude = 4.88969,
				FeatureClass = "P",
				FeatureCode = "PPLC",
				CountryCode = "NL",
				Cc2 = "",
				Admin1Code = "07",
				Admin2Code = "0363",
				Admin3Code = "",
				Admin4Code = "",
				Province = "Noord-Holland",
				Population = 741636,
				Elevation = null,
				Dem = 13,
				TimeZoneId = "Europe/Amsterdam",
				ModificationDate = new DateOnly(2025, 7, 22),
				CountryName = "Netherlands",
				CountryThreeLetterCode = "NLD",
			});
			dbContext.GeoNameCities.Add(new GeoNameCity
			{
				GeonameId = 3513090,
				Name = "Willemstad",
				AsciiName = "Willemstad",
				AlternateNames =
					"Vilemstad,Vilemstadas,Vilemstade,Vilemstado,Villemstad,Villemstant," +
					"Villemştad,Willemstad,Willemsted,Willemstêd,billemseutateu," +
					"uiremusutatto,vilemastada,villemstatu,wei lian si ta de," +
					"willems tad,wylmstad,Βίλλεμσταντ,Вилемстад,Виллемстад," +
					"Вілемстад,Віллемстад,וילמסטאד,ويلمستاد,ویلمستاد," +
					"ویلمسٹیڈ,विलेमश्टाड,வில்லெம்ஸ்டாடு,วิลเลมสตัด," +
					"ვილેમસ્ટાડી,ウィレムスタット,威廉斯塔德,빌렘스타트",
				Latitude = 12.12246,
				Longitude = -68.88641,
				FeatureClass = "P",
				FeatureCode = "PPLC",
				CountryCode = "CW",
				Cc2 = "",
				Admin1Code = "",
				Admin2Code = "",
				Admin3Code = "",
				Admin4Code = "",
				Province = "",
				Population = 125000,
				Elevation = null,
				Dem = 1,
				TimeZoneId = "America/Curacao",
				ModificationDate = new DateOnly(2024, 1, 10)
			});
			await dbContext.SaveChangesAsync(CancellationToken.None);
		}

		var appSettings = new AppSettings();
		var fakeSelectorStorage = new FakeSelectorStorage();
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeMemoryCache = new FakeMemoryCache();
		var geoNamesCitiesQuery = new GeoNamesCitiesQuery(dbContext, null!);
		var geoNameCitySeedService = new GeoNameCitySeedService(
			fakeSelectorStorage,
			appSettings,
			fakeGeoFileDownload,
			geoNamesCitiesQuery,
			new FakeIWebLogger(),
			fakeMemoryCache
		);
		var fakeService =
			new LocationNameService(geoNamesCitiesQuery, geoNameCitySeedService, null!);
		var controller = new GeoLocationNameController(fakeService);
		return ( controller, fakeGeoFileDownload );
	}

	[TestMethod]
	[DataRow("2026-01-30T12:00:00", "Amsterdam", DisplayName = "Winter time format")]
	[DataRow("2026-06-30T12:00:00", "Amsterdam", DisplayName = "Summer time format")]
	[DataRow("2026-06-30T12:00:00", "Willemstad", DisplayName = "No seasonal time change")]
	[DataRow("2026-02-01T14:15:21.659Z", "Amsterdam", DisplayName = "Winter time format with Z")]
	public async Task SearchCityTimezone_ReturnsOk_Verify(string dateTime,
		string cityName)
	{
		// Arrange
		await using var dbContext = CreateDbContext();
		var (controller, fakeGeoFileDownload) = await CreateController(dbContext);

		// Act
		var result = await controller.SearchCityTimezone(dateTime, cityName);

		// Assert
		var okResult = result as OkObjectResult;
		Assert.IsNotNull(okResult);
		Assert.AreEqual(200, okResult.StatusCode ?? 200);
		Assert.IsNotNull(okResult.Value);

		var cityTimezoneResults = okResult.Value as List<CityTimezoneResult>;
		Assert.IsNotNull(cityTimezoneResults);
		Assert.HasCount(1, cityTimezoneResults);
		Assert.AreEqual(1, fakeGeoFileDownload.Count);
		await Verify(cityTimezoneResults).UseParameters(dateTime, cityName);
	}

	[TestMethod]
	public async Task SearchCityTimezone_InvalidModel_ReturnsBadRequest()
	{
		var fakeService = new FakeLocationNameService();
		var controller = new GeoLocationNameController(fakeService);
		controller.ModelState.AddModelError("city", "Required");

		var result = await controller.SearchCityTimezone("2026-01-30T12:00:00Z", "Amsterdam");
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public async Task SearchCity_InvalidModel_ReturnsBadRequest()
	{
		var fakeService = new FakeLocationNameService();
		var controller = new GeoLocationNameController(fakeService);
		controller.ModelState.AddModelError("city", "Required");

		var result = await controller.SearchCity("Amsterdam");
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}
}
