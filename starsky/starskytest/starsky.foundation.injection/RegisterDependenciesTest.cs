using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.injection;

namespace starskytest.starsky.foundation.injection;

[TestClass]
public sealed class RegisterDependenciesTest
{
	[TestMethod]
	public void Configure_NoRedirectClient_DoesNotFollowRedirectsAndHasUserAgent()
	{
		var services = new ServiceCollection();
		RegisterDependencies.Configure(services);
		using var provider = services.BuildServiceProvider();
		var factory = provider.GetRequiredService<IHttpClientFactory>();

		using var client = factory.CreateClient("starsky-no-redirect");
		Assert.Contains("Mozilla", client.DefaultRequestHeaders.UserAgent.ToString());

		// the handler is wrapped by the factory; walk down to the primary handler
		var handler = ( HttpClientHandler ) FindPrimaryHandler(factory, "starsky-no-redirect");
		Assert.IsFalse(handler.AllowAutoRedirect);
	}

	private static HttpMessageHandler FindPrimaryHandler(IHttpClientFactory factory, string name)
	{
		var createHandler = factory.GetType().GetMethod("CreateHandler",
			BindingFlags.Public | BindingFlags.Instance)!;
		HttpMessageHandler? current = ( HttpMessageHandler ) createHandler.Invoke(factory, [name])!;
		while ( current is DelegatingHandler delegating )
		{
			current = delegating.InnerHandler;
		}

		return current!;
	}
}
