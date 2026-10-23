using System.Collections.Generic;

namespace starskytest.starsky.feature.geolookup.Services;

/// <summary>
///     Places with the result a user expects. Used by the recorded (fixture) tests that run
///     always and by the live tests that are disabled by default.
/// </summary>
internal static class OsmMapElementsExpectations
{
	internal sealed record Place(
		string Name,
		double Latitude,
		double Longitude,
		string? NearbyFirst = null,
		string? EnclosingFirst = null,
		string[]? NearbyContains = null,
		string[]? EnclosingContains = null,
		bool NearbyEmpty = false);

	internal static readonly IReadOnlyList<Place> Places =
	[
		// mountain top: the peak is the first nearby object
		new("tete-de-porte", 46.046546, 6.752602, "Tête de Porte"),
		// inside a reservoir, nothing within 20 meters
		new("lac-d-emosson", 46.08353, 6.912979, null, "Lac d'Emosson", null, null, true),
		new("worpplantsoen", 52.25044, 6.147579, null, "Worpplantsoen"),
		// on a bridge: named first in enclosing, further down in nearby
		new("wilhelminabrug", 52.248773, 6.157151, null, "Wilhelminabrug",
			["Wilhelminabrug"]),
		// a generic unnamed building=yes should not be shown as 'Yes'
		new("zoo-berlin", 52.508364, 13.338982, null, "Zoo Berlin"),
		// 100+ nearby footways and steps should not push the station out
		new("hauptbahnhof-berlin", 52.525223, 13.370114, "Hauptbahnhof"),
		new("brandenburger-tor", 52.5163, 13.3777, null, null, ["Brandenburger Tor"]),
		new("matterhorn", 45.97642, 7.65853, "Matterhorn"),
		new("eiffeltoren", 48.85837, 2.29448, null, null, null, ["Tour Eiffel"])
	];

	internal static IEnumerable<object[]> PlaceNames()
	{
		foreach ( var place in Places )
		{
			yield return [place.Name];
		}
	}
}
