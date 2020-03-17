using System.Globalization;

namespace starsky.foundation.readmeta.Helpers
{
	public class MathFraction
	{
		/// <summary>
		/// Get the output of a fraction string
		/// for example: 1/8 become 0.125
		/// </summary>
		/// <param name="value">1/8</param>
		/// <returns></returns>
		public double Fraction(string value)
		{
			var gpsAltitudeValues = value.Split("/".ToCharArray());
			if(gpsAltitudeValues.Length != 2) return 0f;
			var numerator = double.Parse(gpsAltitudeValues[0], CultureInfo.InvariantCulture);
			var denominator = double.Parse(gpsAltitudeValues[1], CultureInfo.InvariantCulture);
			return numerator / denominator;
		}
	}
}
