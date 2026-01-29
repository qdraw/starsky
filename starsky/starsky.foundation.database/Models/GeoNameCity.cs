using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeZoneConverter;

namespace starsky.foundation.database.Models;

public sealed class GeoNameCity
{
	[Key] public int GeonameId { get; set; }

	[Required] [MaxLength(200)] public required string Name { get; set; } = "";

	[MaxLength(200)] public string AsciiName { get; set; } = "";
	[MaxLength(1000)] public string AlternateNames { get; set; } = "";

	[Column(TypeName = "float")] public double Latitude { get; set; }
	[Column(TypeName = "float")] public double Longitude { get; set; }

	[MaxLength(1)] public string FeatureClass { get; set; } = "";
	[MaxLength(10)] public string FeatureCode { get; set; } = "";

	[MaxLength(2)] public string CountryCode { get; set; } = "";
	[MaxLength(60)] public string Cc2 { get; set; } = "";

	[MaxLength(20)] public string Admin1Code { get; set; } = "";
	[MaxLength(80)] public string Admin2Code { get; set; } = "";
	[MaxLength(20)] public string Admin3Code { get; set; } = "";
	[MaxLength(20)] public string Admin4Code { get; set; } = "";

	public long Population { get; set; }

	public int? Elevation { get; set; }
	public int Dem { get; set; }

	[MaxLength(40)] public string TimeZoneId { get; set; } = "";

	[NotMapped]
	public TimeZoneInfo TimeZone
	{
		get
		{
			try
			{
				return TZConvert.GetTimeZoneInfo(TimeZoneId);
			}
			catch ( Exception )
			{
				return TimeZoneInfo.Utc;
			}
		}
	}

	public DateOnly ModificationDate { get; set; }
}
