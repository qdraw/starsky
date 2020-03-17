using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace starsky.foundation.readmeta.Helpers
{
    public static class GeoDistanceTo
    {
        /// <summary>
        /// Calculate straight line distance in kilometers
        /// </summary>
        /// <param name="latitudeFrom">latitude in decimal degrees</param>
        /// <param name="longitudeFrom">longitude in decimal degrees</param>
        /// <param name="latitudeTo">latitude in decimal degrees</param>
        /// <param name="longitudeTo">longitude in decimal degrees</param>
        /// <returns>in kilometers</returns>
        public static double GetDistance(double latitudeFrom, double longitudeFrom, double latitudeTo, double longitudeTo)
        {
            double dlon = Radians(longitudeTo - longitudeFrom);
            double dlat = Radians(latitudeTo - latitudeFrom);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + 
                       Math.Cos(Radians(latitudeFrom)) * Math.Cos(Radians(latitudeTo)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return angle * Radius;
        }
        
	    /// <summary>
	    /// Radius of the earth
	    /// </summary>
        private const double Radius = 6378.16;

        /// <summary>
        /// Convert degrees to Radians
        /// </summary>
        /// <param name="x">Degrees</param>
        /// <returns>The equivalent in radians</returns>
        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }
	    
	    /// <summary>
	    /// Convert 17.21.18S / DD°MM’SS.s” usage to double
	    /// </summary>
	    /// <param name="point"></param>
	    /// <param name="refGps"></param>
	    /// <returns></returns>
	    public static double ConvertDegreeMinutesSecondsToDouble(string point, string refGps)
	    {
		    //Example: 17.21.18S
		    // DD°MM’SS.s” usage
            
		    var multiplier = (refGps.Contains("S") || refGps.Contains("W")) ? -1 : 1; //handle south and west

		    point = Regex.Replace(point, "[^0-9\\., ]", "", RegexOptions.CultureInvariant); //remove the characters

		    // When you use an localisation where commas are used instead of a dot
		    point = point.Replace(",", ".");

		    var pointArray = point.Split(' '); //split the string.

		    //Decimal degrees = 
		    //   whole number of degrees, 
		    //   plus minutes divided by 60, 
		    //   plus seconds divided by 3600

		    var degrees = double.Parse(pointArray[0], CultureInfo.InvariantCulture);
		    var minutes = double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;
		    var seconds = double.Parse(pointArray[2],CultureInfo.InvariantCulture) / 3600;

		    return (degrees + minutes + seconds) * multiplier;
	    }

	    /// <summary>
	    /// Convert "5,55.840,E" to double
	    /// </summary>
	    /// <param name="point"></param>
	    /// <param name="refGps"></param>
	    /// <returns></returns>
	    public static double ConvertDegreeMinutesToDouble(string point, string refGps)
	    {
		    // "5,55.840E"
		    var multiplier = (refGps.Contains("S") || refGps.Contains("W")) ? -1 : 1; //handle south and west

		    point = point.Replace(",", " ");
		    point = Regex.Replace(point, "[^0-9\\., ]", "", RegexOptions.CultureInvariant); //remove the characters

		    var pointArray = point.Split(' '); //split the string.
		    var degrees = double.Parse(pointArray[0], CultureInfo.InvariantCulture);
		    var minutes = double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;
            
		    return (degrees + minutes) * multiplier;
	    }
    }
}
