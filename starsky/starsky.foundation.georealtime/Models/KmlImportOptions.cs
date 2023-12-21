using System.Collections.Generic;

namespace starsky.foundation.georealtime.Models;

public class KmlImportOptions
{
	public string OutputPath { get; set; } = string.Empty;
	public bool OutputPathSubPath { get; set; } = true;

	public bool SplitByDay { get; set; }

	public bool OutputGeoJson { get; set; } = false;
	public bool OutputGeoJsonPoints { get; set; } = false;
	public bool OutputGpx { get; set; } = true;

	public List<LatitudeLongitudeModel> Filter { get; set; }
}
