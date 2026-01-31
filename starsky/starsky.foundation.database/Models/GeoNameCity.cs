using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeZoneConverter;

namespace starsky.foundation.database.Models;

public sealed class GeoNameCity
{
	/// <summary>
	///     integer id of record in geonames database
	/// </summary>
	[Key]
	public int GeonameId { get; set; }

	/// <summary>
	///     name of geographical point (utf8) varchar(200)
	/// </summary>
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; } = "";

	/// <summary>
	///     name of geographical point in plain ascii characters, varchar(200)
	/// </summary>
	[MaxLength(200)]
	public string AsciiName { get; set; } = "";

	/// <summary>
	///     alternatenames, comma separated, ascii names automatically transliterated, convenience
	///     attribute from alternatename table, varchar(10000)
	/// </summary>
	[MaxLength(1000)]
	public string AlternateNames { get; set; } = "";

	/// <summary>
	///     latitude in decimal degrees (wgs84)
	/// </summary>
	[Column(TypeName = "float")]
	public double Latitude { get; set; }

	/// <summary>
	///     longitude in decimal degrees (wgs84)
	/// </summary>
	[Column(TypeName = "float")]
	public double Longitude { get; set; }

	/// <summary>
	///     see http://www.geonames.org/export/codes.html, char(1)
	/// </summary>
	[MaxLength(1)]
	public string FeatureClass { get; set; } = "";

	/// <summary>
	///     see http://www.geonames.org/export/codes.html, varchar(10)
	/// </summary>
	[MaxLength(10)]
	public string FeatureCode { get; set; } = "";

	/// <summary>
	///     ISO-3166 2-letter country code, 2 characters
	/// </summary>
	[MaxLength(2)]
	public string CountryCode { get; set; } = "";

	/// <summary>
	///     alternate country codes, comma separated, ISO-3166 2-letter country code, 200 characters
	/// </summary>
	[MaxLength(60)]
	public string Cc2 { get; set; } = "";

	/// <summary>
	///     fipscode (subject to change to iso code),
	///     see exceptions below, see file admin1Codes.txt for display names of this code; varchar(20)
	///     AdminCodes:
	///     Most adm1 are FIPS codes. ISO codes are used for US, CH, BE and ME. UK and Greece are
	///     using an additional level between country and fips code. The code '00' stands for general
	///     features where no specific adm1 code is defined.
	///     The corresponding admin feature is found with the same countrycode and adminX codes and the
	///     respective feature code ADMx.
	/// </summary>
	[MaxLength(20)]
	public string Admin1Code { get; set; } = "";

	/// <summary>
	///     code for the second administrative division, a county in the US, see file admin2Codes.txt;
	///     varchar(80)
	/// </summary>
	[MaxLength(80)]
	public string Admin2Code { get; set; } = "";

	/// <summary>
	///     code for third level administrative division, varchar(20)
	/// </summary>
	[MaxLength(20)]
	public string Admin3Code { get; set; } = "";

	/// <summary>
	///     code for fourth level administrative division, varchar(20)
	/// </summary>
	[MaxLength(20)]
	public string Admin4Code { get; set; } = "";

	/// <summary>
	///     bigint (8 byte int)
	/// </summary>
	public long Population { get; set; }

	/// <summary>
	///     in meters, integer
	/// </summary>
	public int? Elevation { get; set; }

	/// <summary>
	///     digital elevation model, srtm3 or gtopo30, average elevation of 3''x3'' (ca 90mx90m)
	///     or 30''x30'' (ca 900mx900m) area in meters, integer. srtm processed by cgiar/ciat.
	/// </summary>
	public int Dem { get; set; }

	/// <summary>
	///     the iana timezone id (see file timeZone.txt) varchar(40)
	/// </summary>
	[MaxLength(40)]
	public string TimeZoneId { get; set; } = "";

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

	/// <summary>
	///     date of last modification in yyyy-MM-dd format
	/// </summary>
	public DateOnly ModificationDate { get; set; }

	/// <summary>
	///     Province in English
	/// </summary>

	[MaxLength(200)]
	public string Province { get; set; } = string.Empty;
}
