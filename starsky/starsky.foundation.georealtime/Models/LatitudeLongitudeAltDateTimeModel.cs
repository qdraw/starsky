using System;

namespace starsky.foundation.georealtime.Models;

public class LatitudeLongitudeAltDateTimeModel : LatitudeLongitudeModel
{
	public string Altitude { get; set; }
	public DateTime DateTime { get; set; }
}
