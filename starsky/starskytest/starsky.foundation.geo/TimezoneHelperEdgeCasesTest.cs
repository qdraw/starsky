using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.geo.TimezoneHelper;
using TimeZoneConverter;

namespace starskytest.starsky.foundation.geo;

[TestClass]
public class TimezoneHelperEdgeCasesTest
{
	[TestMethod]
	[DataRow("Asia/Amman", "2026-03-28T23:00:00Z", false)] // DST not observed in 2026
	[DataRow("Asia/Amman", "2026-10-30T23:00:00Z", false)] // DST not observed in 2026
	[DataRow("Europe/Moscow", "2026-01-01T00:00:00Z", false)] // No DST in Moscow
	[DataRow("America/Sao_Paulo", "2026-01-01T00:00:00Z", false)] // No DST in Brazil (abolished)
	[DataRow("America/Argentina/Buenos_Aires", "2026-01-01T00:00:00Z",
		false)] // No DST in Argentina
	// Turkey used DST until 2016, then switched to permanent UTC+3
	// Mid-year during DST: a future offset change (fall back) will still occur
	[DataRow("Europe/Istanbul", "2015-06-01T00:00:00Z", true)]
	// After DST abolition: no future DST changes anymore
	[DataRow("Europe/Istanbul", "2017-01-01T00:00:00Z", false)]
	[DataRow("Europe/Istanbul", "2025-01-01T00:00:00Z", false)]
	// DST is active, but the offset change is only 30 minutes
	[DataRow("Australia/Lord_Howe", "2023-12-01T00:00:00Z", true)]
	public void IsDst_EdgeCases(string tzId, string dateTimeUtc, bool expected)
	{
		var tz = TZConvert.GetTimeZoneInfo(tzId);
		var date = DateTime.Parse(dateTimeUtc, CultureInfo.InvariantCulture,
			DateTimeStyles.AdjustToUniversal);
		var result = tz.IsDst(date);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	[DataRow("Asia/Amman", "2026-03-28T23:00:00Z", false)] // No future DST rules in 2026
	[DataRow("Asia/Amman", "2026-10-30T23:00:00Z", false)] // No future DST rules in 2026
	[DataRow("Europe/Moscow", "2026-01-01T00:00:00Z", false)] // No DST rules
	[DataRow("America/Sao_Paulo", "2026-01-01T00:00:00Z", false)] // No DST rules
	[DataRow("America/Argentina/Buenos_Aires", "2026-01-01T00:00:00Z", false)] // No DST rules
	// Winter before DST abolition: standard time
	[DataRow("Europe/Istanbul", "2015-01-01T12:00:00Z", false)]
	// Summer after DST abolition: permanent UTC+3, no DST
	[DataRow("Europe/Istanbul", "2018-07-01T12:00:00Z", false)]
	// Winter after DST abolition: still no DST
	[DataRow("Europe/Istanbul", "2024-01-01T12:00:00Z", false)]
	// DST is active, but the offset change is only 30 minutes
	[DataRow("Australia/Lord_Howe", "2023-12-01T00:00:00Z", true)]
	public void HasFutureDst_EdgeCases(string tzId, string dateTimeUtc, bool expected)
	{
		var tz = TZConvert.GetTimeZoneInfo(tzId);
		
		foreach (var rule in tz.GetAdjustmentRules())
		{
			Console.WriteLine($"Rule: Start={rule.DateStart}, End={rule.DateEnd}, Delta={rule.DaylightDelta}");
		}
		
		var date = DateTime.Parse(dateTimeUtc, CultureInfo.InvariantCulture,
			DateTimeStyles.AdjustToUniversal);
		Console.WriteLine(
			$"Offset: {tz.GetUtcOffset(date)}, Is DST: {tz.IsDaylightSavingTime(TimeZoneInfo.ConvertTimeFromUtc(date, tz))}");

		var result = tz.HasFutureDst(date);
		Assert.AreEqual(expected, result);
	}
}
