using System;
using System.Globalization;
using System.Text.RegularExpressions;
using starsky.foundation.readmeta.Models;

namespace starsky.foundation.readmeta.Helpers
{
	internal static partial class GeoParserInvalidCharactersRegex
	{
		/// <summary>
		///  Generated regex for removing invalid characters
		/// </summary>
		/// <returns></returns>
		[GeneratedRegex("[^0-9\\., ]",
			RegexOptions.CultureInvariant,
			matchTimeoutMilliseconds: 1000)]
		public static partial Regex GetRegex();
	}

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

			var multiplier = ( refGps.Contains('S') || refGps.Contains('W') ) ? -1 : 1; // handle south and west

			point = GeoParserInvalidCharactersRegex.GetRegex().Replace(point, ""); // remove the characters

			// When you use a localisation where commas are used instead of a dot
			point = point.Replace(",", ".");

			var pointArray = point.Split(' '); //split the string.

			//Decimal degrees = 
			//   whole number of degrees, 
			//   plus minutes divided by 60, 
			//   plus seconds divided by 3600

			var degrees = double.Parse(pointArray[0], CultureInfo.InvariantCulture);
			var minutes = double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;
			var seconds = double.Parse(pointArray[2], CultureInfo.InvariantCulture) / 3600;

			return ( degrees + minutes + seconds ) * multiplier;
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
			var multiplier = ( refGps.Contains('S') || refGps.Contains('W') ) ? -1 : 1; // handle south and west

			point = point.Replace(",", " ");
			point = GeoParserInvalidCharactersRegex.GetRegex().Replace(point, ""); // remove the characters

			var pointArray = point.Split(' '); //split the string.
			var degrees = double.Parse(pointArray[0], CultureInfo.InvariantCulture);
			var minutes = double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;

			return ( degrees + minutes ) * multiplier;
		}

		private static readonly char[] Separator = ['+', '-'];

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
			if ( isoStr.Length < 18 || !isoStr.EndsWith('/') )
			{
				return geoListItem;
			}

			isoStr = isoStr.Remove(isoStr.Length - 1); // Remove trailing slash

			var parts = isoStr.Split(Separator, StringSplitOptions.None);
			if ( parts.Length is < 3 or > 4 )
			{
				// Check for parts count
				return geoListItem;
			}

			if ( parts[0].Length != 0 )
			{
				// Check if first part is empty
				return geoListItem;
			}

			var point = parts[1].IndexOf('.');
			if ( point != 2 && point != 4 && point != 6 )
			{
				// Check for valid length for lat/lon
				return geoListItem;
			}

			if ( point != parts[2].IndexOf('.') - 1 )
			{
				// Check for lat/lon decimal positions
				return geoListItem;
			}

			var numberFormatInfo = NumberFormatInfo.InvariantInfo;

			switch ( point )
			{
				// Parse latitude and longitude values, according to format
				case 2:
					geoListItem.Latitude = float.Parse(parts[1], numberFormatInfo) * 3600;
					geoListItem.Longitude = float.Parse(parts[2], numberFormatInfo) * 3600;
					break;
				case 4:
					geoListItem.Latitude = float.Parse(parts[1].AsSpan(0, 2), numberFormatInfo) * 3600 +
										   float.Parse(parts[1].AsSpan(2), numberFormatInfo) * 60;
					geoListItem.Longitude = float.Parse(parts[2].AsSpan(0, 3), numberFormatInfo) * 3600 +
											float.Parse(parts[2].AsSpan(3), numberFormatInfo) * 60;
					break;
				// point==8 / 6
				default:
					geoListItem.Latitude = float.Parse(parts[1].AsSpan(0, 2), numberFormatInfo) * 3600 +
										   float.Parse(parts[1].AsSpan(2, 2), numberFormatInfo) * 60 +
										   float.Parse(parts[1].AsSpan(4), numberFormatInfo);
					geoListItem.Longitude = float.Parse(parts[2].AsSpan(0, 3), numberFormatInfo) * 3600 +
											float.Parse(parts[2].AsSpan(3, 2), numberFormatInfo) * 60 +
											float.Parse(parts[2].AsSpan(5), numberFormatInfo);
					break;
			}

			// Parse altitude, just to check if it is valid
			if ( parts.Length == 4 && !float.TryParse(parts[3],
					NumberStyles.Float, numberFormatInfo, out _) )
			{
				return geoListItem;
			}

			// Add proper sign to lat/lon
			if ( isoStr[0] == '-' )
			{
				geoListItem.Latitude = -geoListItem.Latitude;
			}

			if ( isoStr[parts[1].Length + 1] == '-' )
			{
				geoListItem.Longitude = -geoListItem.Longitude;
			}

			// and calc back to degrees
			geoListItem.Latitude /= 3600.0f;
			geoListItem.Longitude /= 3600.0f;

			return geoListItem;
		}
	}
}
