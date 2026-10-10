namespace starsky.foundation.platform.Models;

/// <summary>
///     Crash report settings, only unhandled exceptions are reported
/// </summary>
public class SentrySettings
{
	/// <summary>
	///     Public ingest key, normally baked into Release builds.
	///     Use this to overwrite it (app__Sentry__Dsn)
	/// </summary>
	public string? Dsn { get; set; }

	/// <summary>
	///     Overwrite the environment value
	/// </summary>
	public string? Environment { get; set; }

	/// <summary>
	///     Give back the Environment or if empty the ASPNETCORE_ENVIRONMENT variable
	/// </summary>
	public string GetEnvironmentName()
	{
		return string.IsNullOrWhiteSpace(Environment)
			? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
			  "production"
			: Environment;
	}
}
