using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.injection;

public static class RegisterDependencies
{
	private const string HttpClientName = "starsky";

	private const string NoRedirectHttpClientName = "starsky-no-redirect";

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
		// used for user supplied urls: redirects must be validated by the caller (SSRF)
		serviceCollection.AddHttpClient(NoRedirectHttpClientName, client =>
		{
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
		}).ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.HttpClientHandler
		{
			AllowAutoRedirect = false
		});
		// change to: *.Project.*", "*.Feature.*" "*.Foundation.*"
		serviceCollection.AddClassesWithServiceAttribute("starsky*");
	}
}
