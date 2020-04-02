using System;
using System.Globalization;
using System.Text.RegularExpressions;
using starsky.foundation.readmeta.Models;

namespace starsky.foundation.readmeta.Helpers
{
	public static class GeoParser
	{
			    
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
	    
		/// <summary>
		/// Parsing method
		/// </summary>
		/// <param name="isoStr"></param>
        public static GeoListItem ParseIsoString(string isoStr)
        {
	        var geoListItem = new GeoListItem();
	        
            // Parse coordinate in the following ISO 6709 formats:
            // source: https://github.com/jaime-olivares/coordinate/blob/master/Coordinate.cs
            // Latitude and Longitude in Degrees:
            // �DD.DDDD�DDD.DDDD/         (eg +12.345-098.765/)
            // Latitude and Longitude in Degrees and Minutes:
            // �DDMM.MMMM�DDDMM.MMMM/     (eg +1234.56-09854.321/)
            // Latitude and Longitude in Degrees, Minutes and Seconds:
            // �DDMMSS.SSSS�DDDMMSS.SSSS/ (eg +123456.7-0985432.1/)
            // Latitude, Longitude (in Degrees) and Altitude:
            // �DD.DDDD�DDD.DDDD�AAA.AAA/         (eg +12.345-098.765+15.9/)
            // Latitude, Longitude (in Degrees and Minutes) and Altitude:
            // �DDMM.MMMM�DDDMM.MMMM�AAA.AAA/     (eg +1234.56-09854.321+15.9/)
            // Latitude, Longitude (in Degrees, Minutes and Seconds) and Altitude:
            // �DDMMSS.SSSS�DDDMMSS.SSSS�AAA.AAA/ (eg +123456.7-0985432.1+15.9/)

            // Check for minimum length
            // Check for trailing slash
            if ( isoStr.Length < 18 || !isoStr.EndsWith("/") )
            {
	            return geoListItem;
            }

            isoStr = isoStr.Remove(isoStr.Length - 1); // Remove trailing slash

            string[] parts = isoStr.Split(new char[] { '+', '-' }, StringSplitOptions.None);
            if (parts.Length < 3 || parts.Length > 4)  // Check for parts count
                parts = null;
            if (parts[0].Length != 0)  // Check if first part is empty
                parts = null;

            int point = parts[1].IndexOf('.');
            if (point != 2 && point != 4 && point != 6) // Check for valid lenght for lat/lon
                parts = null;
            if (point != parts[2].IndexOf('.') - 1) // Check for lat/lon decimal positions
                parts = null;

            if ( parts == null ) return geoListItem;
            
            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo; 

            // Parse latitude and longitude values, according to format
            if (point == 2)
            {
	            geoListItem.Latitude = float.Parse(parts[1], fi) * 3600;
	            geoListItem.Longitude = float.Parse(parts[2], fi) * 3600;
            }
            else if (point == 4)
            {
	            geoListItem.Latitude = float.Parse(parts[1].Substring(0, 2), fi) * 3600 + 
	                                   float.Parse(parts[1].Substring(2), fi) * 60;
	            geoListItem.Longitude = float.Parse(parts[2].Substring(0, 3), fi) * 3600 + 
	                                    float.Parse(parts[2].Substring(3), fi) * 60;
            }
            else  // point==8
            {
	            geoListItem.Latitude = float.Parse(parts[1].Substring(0, 2), fi) * 3600 + 
	                                   float.Parse(parts[1].Substring(2, 2), fi) * 60 + float.Parse(parts[1].Substring(4), fi);
	            geoListItem.Longitude = float.Parse(parts[2].Substring(0, 3), fi) * 3600 + 
	                                    float.Parse(parts[2].Substring(3, 2), fi) * 60 + float.Parse(parts[2].Substring(5), fi);
            }
            
            // Parse altitude, just to check if it is valid
            if (parts.Length == 4)
                float.Parse(parts[3], fi);

            // Add proper sign to lat/lon
            if (isoStr[0] == '-')
	            geoListItem.Latitude = - geoListItem.Latitude;
            if (isoStr[parts[1].Length + 1] == '-')
	            geoListItem.Longitude = - geoListItem.Longitude;

            // and calc back to degrees
            geoListItem.Latitude /= 3600.0f;
            geoListItem.Longitude /= 3600.0f;

            return geoListItem;
        }
	}
}
