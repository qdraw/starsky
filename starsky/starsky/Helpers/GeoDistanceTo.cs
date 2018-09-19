using System;

namespace starsky.Helpers
{
    public static class GeoDistanceTo
    {

        /// <summary>
        /// Calculate distance in meters
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="long1"></param>
        /// <param name="lat2"></param>
        /// <param name="long2"></param>
        /// <returns>int in meters</returns>
        public static int GetDistance(double lat1, double long1, double lat2, double long2)
        {
            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;
 
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                       + Math.Cos(lat2) * Math.Sin(dLong/2) * Math.Sin(dLong/2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Calculate radius of earth
            // For this you can assume any of the two points.
            double radiusE = 6378135; // Equatorial radius, in metres
            double radiusP = 6356750; // Polar Radius

            // Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            // Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                        + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            // Calaculate distance in metres.
            var distance = radius * c;
            return (int) distance;
        }
    }
}
