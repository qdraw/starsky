using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;

namespace starskytest.starsky.feature.geolookup.Services;

/// <summary>
///     Runs OsmMapElementsService on recorded Overpass responses (OsmMapElementsFixtures),
///     so the ranking is tested with real OpenStreetMap data without a network connection.
///     Refresh the recordings with capture-fixtures.js
/// </summary>
[TestClass]
public class OsmMapElementsFixtureTest
{
	[TestMethod]
	[DynamicData(nameof(PlaceNames))]
	public async Task LookupAsync_RecordedLiveData_ReturnsExpectedObjects(string name)
	{
		var place = OsmMapElementsExpectations.Places.Single(p => p.Name == name);
		var fakeHttp = OsmMapElementsTestHelper.FakeHttpWithFixture(place.Name, place.Latitude,
			place.Longitude);

		var result = await new OsmMapElementsService(fakeHttp).LookupAsync(place.Latitude,
			place.Longitude);

		OsmMapElementsTestHelper.AssertPlace(place, result);
	}

	[TestMethod]
	public async Task LookupAsync_Hauptbahnhof_StationRanksBeforeFootways()
	{
		var place = OsmMapElementsExpectations.Places.Single(p => p.Name == "hauptbahnhof-berlin");
		var fakeHttp = OsmMapElementsTestHelper.FakeHttpWithFixture(place.Name, place.Latitude,
			place.Longitude);

		var result = await new OsmMapElementsService(fakeHttp).LookupAsync(place.Latitude,
			place.Longitude);

		Assert.IsFalse(result.NearbyObjects.Any(p => p.Label == "Footway"),
			"unnamed footways come after the named objects");
		Assert.IsTrue(result.EnclosingObjects.Any(p => p.Label == "Hauptbahnhof"));
	}

	[TestMethod]
	public async Task LookupAsync_ZooBerlin_DoesNotShowGenericBuildingAsYes()
	{
		var place = OsmMapElementsExpectations.Places.Single(p => p.Name == "zoo-berlin");
		var fakeHttp = OsmMapElementsTestHelper.FakeHttpWithFixture(place.Name, place.Latitude,
			place.Longitude);

		var result = await new OsmMapElementsService(fakeHttp).LookupAsync(place.Latitude,
			place.Longitude);

		Assert.IsFalse(result.NearbyObjects.Concat(result.EnclosingObjects)
			.Any(p => p.Label == "Yes"));
		Assert.AreEqual("Zoo Berlin", result.EnclosingObjects[0].Label);
		Assert.AreEqual("tourism", result.EnclosingObjects[0].Category);
		Assert.AreEqual("zoo", result.EnclosingObjects[0].Type);
	}

	public static System.Collections.Generic.IEnumerable<object[]> PlaceNames =>
		OsmMapElementsExpectations.PlaceNames();
}
