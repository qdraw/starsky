using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.injection;

public static class RegisterDependencies
{
	/// <summary>
	///     Run through the entire solution and add Dependency injection
	///     Need to build afterward
	/// </summary>
	/// <param name="serviceCollection">the ASP.Net service collection</param>
	public static void Configure(IServiceCollection serviceCollection)
	{
		// required by IHttpClientHelper
		serviceCollection.AddSingleton<HttpClient>();
		// change to: *.Project.*", "*.Feature.*" "*.Foundation.*"
		serviceCollection.AddClassesWithServiceAttribute("starsky*");
	}
}
