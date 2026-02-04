using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;

namespace starskytest.starsky.feature.geolookup.Services;

[TestClass]
public class GeoFolderReverseLookupTests
{
	[TestMethod]
	public void RemoveNoUpdateItems_OverwriteLocationNamesTrue_ReturnsOnlyItemsWithGps()
	{
		var items = new List<FileIndexItem>
		{
			new() { Latitude = 1.0, Longitude = 1.0, FileName = "a.jpg" },
			new() { Latitude = 0.0, Longitude = 1.0, FileName = "b.jpg" },
			new() { Latitude = 1.0, Longitude = 0.0, FileName = "c.jpg" }
		};

		var result = GeoFolderReverseLookup.RemoveNoUpdateItems(items, true);

		Assert.HasCount(1, result);
		Assert.AreEqual("a.jpg", result[0].FileName);
	}

	[TestMethod]
	public void RemoveNoUpdateItems_OverwriteLocationNamesFalse_FiltersCorrectly()
	{
		var items = new List<FileIndexItem>
		{
			new()
			{
				Latitude = 1.0,
				Longitude = 1.0,
				LocationCity = null,
				LocationCountryCode = null,
				LocationCountry = null,
				FileName = "file1.jpg"
			},
			new()
			{
				Latitude = 1.0,
				Longitude = 1.0,
				LocationCity = "City",
				LocationCountryCode = "NLD",
				LocationCountry = "Netherlands",
				FileName = "file2.jpg"
			},
			new()
			{
				Latitude = 0.0,
				Longitude = 0.0,
				LocationCity = null,
				LocationCountryCode = null,
				LocationCountry = null,
				FileName = "file3.jpg"
			}
		};

		var result = GeoFolderReverseLookup.RemoveNoUpdateItems(items, false);

		Assert.HasCount(1, result);
		Assert.AreEqual("file1.jpg", result[0].FileName);
	}

	[TestMethod]
	public void RemoveNoUpdateItems_EmptyList_ReturnsEmptyList()
	{
		var result = GeoFolderReverseLookup.RemoveNoUpdateItems(new List<FileIndexItem>(), true);

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void RemoveNoUpdateItems_AllItemsFiltered_ReturnsEmptyList()
	{
		var items = new List<FileIndexItem>
		{
			new() { Latitude = 0.0, Longitude = 0.0, FileName = "file.jpg" }
		};

		var result = GeoFolderReverseLookup.RemoveNoUpdateItems(items, true);

		Assert.IsEmpty(result);
	}
}
