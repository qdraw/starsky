using System.Globalization;

namespace starskycore.Helpers
{
	public class MathFraction
	{
		public double Fraction(string gpsAltitude)
		{
			var gpsAltitudeValues = gpsAltitude.Split("/".ToCharArray());
			if(gpsAltitudeValues.Length != 2) return 0f;
			var numerator = double.Parse(gpsAltitudeValues[0], CultureInfo.InvariantCulture);
			var denominator = double.Parse(gpsAltitudeValues[1], CultureInfo.InvariantCulture);
			return numerator / denominator;
		}
	}
}
