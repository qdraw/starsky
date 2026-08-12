using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.injection;

public static class RegisterDependencies
{
	private const string HttpClientName = "starsky";

	private const string UserAgent =
		"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

	/// <summary>
	///     Run through the entire solution and add Dependency injection
	///     Need to build afterward
	/// </summary>
	/// <param name="serviceCollection">the ASP.Net service collection</param>
	public static void Configure(IServiceCollection serviceCollection)
	{
		serviceCollection.AddHttpClient(HttpClientName, client =>
		{
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
		});
		// change to: *.Project.*", "*.Feature.*" "*.Foundation.*"
		serviceCollection.AddClassesWithServiceAttribute("starsky*");
	}
}
