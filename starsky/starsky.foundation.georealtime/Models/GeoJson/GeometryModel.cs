using System;
using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.georealtime.Models.GeoJson;

public class GeometryModel
{
	public GeometryType? Type { get; set; }
	
	private List<List<double>> CoordinatesPrivate { get; set; }
	
	public List<List<double>> Coordinates
	{
		get => CoordinatesPrivate;
		set
		{
			if ( value.Any(coordinatesArray => coordinatesArray.Count != 2 && coordinatesArray.Count != 3) )
			{
				throw new ArgumentException("Coordinates must be 2 or 3 dimensional");
			}

			CoordinatesPrivate = value;
		}
	}

}
