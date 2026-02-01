namespace starsky.foundation.geo.TimezoneHelper;

public static class TimezoneHelper
{
	public static bool HasFutureDst(this TimeZoneInfo tz, DateTime fromUtc)
	{
		var start = fromUtc;

		var rules = tz.GetAdjustmentRules();

		return rules.Any(r =>
			r.DateEnd.ToUniversalTime() >= start &&
			r.DaylightDelta != TimeSpan.Zero
		);
	}

	public static bool IsDst(this TimeZoneInfo tz, DateTime utcDateTime)
	{
		var local = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
		return tz.IsDaylightSavingTime(local);
	}
}
