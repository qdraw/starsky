using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;

namespace starsky.foundation.http
{
	[Service(InjectionLifetime = InjectionLifetime.Scoped)]
	public class RegisterHttpDependencies
	{
		public RegisterHttpDependencies(IServiceCollection serviceCollection)
		{
			// required by IHttpClientHelper
			serviceCollection.AddScoped<System.Net.Http.HttpClient>();
			// for example needed by Application Insights
			serviceCollection.AddSingleton<IHttpContextAccessor>();
		}
	}
}
