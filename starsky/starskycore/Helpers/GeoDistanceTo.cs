using System;

namespace starskycore.Helpers
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
    }
}
