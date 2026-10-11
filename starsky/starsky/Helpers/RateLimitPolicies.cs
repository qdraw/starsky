namespace starsky.Helpers;

/// <summary>
///     Names of the rate limit policies that are registered in Startup.AddRateLimiting
/// </summary>
public static class RateLimitPolicies
{
	/// <summary>
	///     Endpoints that can be used without a login and do work on the server
	/// </summary>
	public const string Anonymous = "anonymous";

	/// <summary>
	///     Login and register: limits password guessing per client
	/// </summary>
	public const string Login = "login";
}
