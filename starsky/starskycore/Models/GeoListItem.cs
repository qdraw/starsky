using System;

namespace starsky.Models
{
    public class GeoListItem
    {
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public DateTime DateTime { get; set; }
    }
}
