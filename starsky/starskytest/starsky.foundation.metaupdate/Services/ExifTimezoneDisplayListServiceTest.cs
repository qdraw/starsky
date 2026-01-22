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
		var list = service.GetMovedToDifferentPlaceTimezonesList();

		Assert.IsNotNull(list);
		Assert.IsNotEmpty(list);
		// Basic presence check: common IDs like "UTC" or display names containing UTC
		var hasUtc = list.Any(x => x.Id == "UTC" || x.DisplayName.Contains("UTC"));
		Assert.IsTrue(hasUtc);
	}
}
