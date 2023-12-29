using System;

namespace starsky.foundation.georealtime.Models;

public class LatitudeLongitudeAltDateTimeModel : LatitudeLongitudeModel
{
	/// <summary>
	/// Altitude in meters
	/// </summary>
	public double? Altitude { get; set; }
	
	/// <summary>
	/// DateTime in UTC
	/// </summary>
	public DateTime DateTime { get; set; }
}
