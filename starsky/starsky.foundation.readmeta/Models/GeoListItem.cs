using System;

namespace starsky.foundation.readmeta.Models
{
    public sealed class GeoListItem
    {
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public DateTime DateTime { get; set; }
    }
}
