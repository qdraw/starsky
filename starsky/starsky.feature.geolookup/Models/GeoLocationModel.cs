namespace starsky.feature.geolookup.Models;

public sealed class GeoLocationModel
{
	public bool IsSuccess { get; set; } = true;

	public double Latitude { get; set; }
	public double Longitude { get; set; }

	public string? LocationCity { get; set; }
	public string? LocationCountry { get; set; }
	public string? LocationCountryCode { get; set; }
	public string? LocationState { get; set; }
	public string? ErrorReason { get; set; }
}
