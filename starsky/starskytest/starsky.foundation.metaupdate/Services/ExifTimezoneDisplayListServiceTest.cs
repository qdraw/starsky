using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metaupdate.Services;

namespace starskytest.starsky.foundation.metaupdate.Services;

[TestClass]
public sealed class ExifTimezoneDisplayListServiceTest
{
	[TestMethod]
	public void GetIncorrectCameraTimezonesList_SkipZero_GeneratesRangeMinus14ToPlus12()
	{
		var service = new ExifTimezoneDisplayListService();
		var list = service.GetIncorrectCameraTimezonesList();

		Assert.IsNotNull(list);
		// Should have 27 entries (-14..-1 and +1..+12)
		Assert.HasCount(27, list);
		// Ensure GMT (0) is skipped -> this should be "Etc/GMT" 
		Assert.IsFalse(list.Any(x => x.Id == "Etc/GMT0"));
		// Contains boundaries
		Assert.IsTrue(list.Any(x => x.Id == "Etc/GMT"));
		Assert.IsTrue(list.Any(x => x.Id == "Etc/GMT-14"));
		Assert.IsTrue(list.Any(x => x.Id == "Etc/GMT+12"));
	}

	[TestMethod]
	[DataRow("Etc/GMT-2", "UTC+02")]
	[DataRow("Etc/GMT+1", "UTC-01")]
	[DataRow("Etc/GMT-10", "UTC+10")]
	[DataRow("Etc/GMT-14", "UTC+14")]
	public void GetIncorrectCameraTimezonesList_InvertedSign_Theory(string tzId,
		string expectedDisplay)
	{
		var service = new ExifTimezoneDisplayListService();
		var list = service.GetIncorrectCameraTimezonesList();

		var entry = list.FirstOrDefault(x => x.Id == tzId);
		Assert.IsNotNull(entry);
		Assert.AreEqual(expectedDisplay, entry.DisplayName);
	}

	[TestMethod]
	public void GetMovedToDifferentPlaceTimezonesList_ContainsSystemZones()
	{
		var service = new ExifTimezoneDisplayListService();
		var testDate = new DateTime(2024, 6, 15,
			0, 0, 0, DateTimeKind.Local); // Summer date
		var list = service.GetMovedToDifferentPlaceTimezonesList(testDate);

		Assert.IsNotNull(list);
		Assert.IsNotEmpty(list);
		// Basic presence check: display names should contain UTC offset
		var hasUtcFormat = list.Any(x => x.DisplayName.Contains("UTC"));
		Assert.IsTrue(hasUtcFormat);
	}

	[TestMethod]
	public void GetMovedToDifferentPlaceTimezonesList_ShowsDSTOffsetInSummer()
	{
		// Arrange
		var service = new ExifTimezoneDisplayListService();
		var summerDate = new DateTime(2024, 7, 15,
			0, 0, 0, DateTimeKind.Local); // Middle of summer

		// Act
		var list = service.GetMovedToDifferentPlaceTimezonesList(summerDate);

		// Assert - For zones with DST, summer offset should be different from standard
		// For example, Central European Time should show UTC+02:00 in summer (DST)
		var cetZone = list.FirstOrDefault(x =>
			x.Id == "Central European Standard Time" ||
			x.Id == "Europe/Amsterdam" ||
			x.Id == "Europe/Berlin");

		if ( cetZone != null )
		{
			// In summer, CET uses UTC+02:00 (CEST)
			Assert.IsTrue(cetZone.DisplayName.Contains("UTC+02:00") ||
			              cetZone.DisplayName.Contains("02:00"),
				$"Expected summer offset, got: {cetZone.DisplayName}");
		}
	}

	[TestMethod]
	public void GetMovedToDifferentPlaceTimezonesList_ShowsStandardOffsetInWinter()
	{
		// Arrange
		var service = new ExifTimezoneDisplayListService();
		var winterDate = new DateTime(2024, 1, 15,
			0, 0, 0, DateTimeKind.Local); // Middle of winter

		// Act
		var list = service.GetMovedToDifferentPlaceTimezonesList(winterDate);

		// Assert - For zones with DST, winter offset should be standard time
		// For example, Central European Time should show UTC+01:00 in winter
		var cetZone = list.FirstOrDefault(x =>
			x.Id == "Central European Standard Time" ||
			x.Id == "Europe/Amsterdam" ||
			x.Id == "Europe/Berlin");

		if ( cetZone != null )
		{
			// In winter, CET uses UTC+01:00 (standard time)
			Assert.IsTrue(cetZone.DisplayName.Contains("UTC+01:00") ||
			              cetZone.DisplayName.Contains("01:00"),
				$"Expected winter offset, got: {cetZone.DisplayName}");
		}
	}

	[TestMethod]
	public void GetMovedToDifferentPlaceTimezonesList_FormatsOffsetCorrectly()
	{
		// Arrange
		var service = new ExifTimezoneDisplayListService();
		var testDate = new DateTime(2024, 6,
			15, 0, 0, 0, DateTimeKind.Local);

		// Act
		var list = service.GetMovedToDifferentPlaceTimezonesList(testDate);

		// Assert - All entries should have (UTC±HH:mm) format in display name
		Assert.IsTrue(list.All(x => x.DisplayName.Contains("UTC")),
			"All display names should contain UTC offset");

		// Check that at least some have the expected format
		var hasPositiveOffset = list.Any(x => x.DisplayName.Contains("UTC+"));
		var hasNegativeOffset = list.Any(x => x.DisplayName.Contains("UTC-"));

		Assert.IsTrue(hasPositiveOffset || hasNegativeOffset,
			"Should have timezones with positive or negative offsets");
	}

	[TestMethod]
	public void GetMovedToDifferentPlaceTimezonesList_MinDateTime_HandlesYearOne()
	{
		var service = new ExifTimezoneDisplayListService();
		var minDate = DateTime.MinValue; // 0001-01-01
		var list = service.GetMovedToDifferentPlaceTimezonesList(minDate);

		Assert.IsNotNull(list);
		Assert.IsNotEmpty(list);
		// All display names should contain UTC offset
		Assert.IsTrue(list.All(x => x.DisplayName.Contains("UTC")),
			"All display names should contain UTC offset");
		// Should not throw or return empty for year 0001
	}
}
