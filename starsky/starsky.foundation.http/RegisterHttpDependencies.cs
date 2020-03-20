using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;

namespace starsky.foundation.http
{
	[Service(InjectionLifetime = InjectionLifetime.Singleton)]
	public class RegisterHttpDependencies
	{
		public RegisterHttpDependencies(IServiceCollection services)
		{
			// required by IHttpClientHelper
			services.AddSingleton<System.Net.Http.HttpClient>();
			// for example needed by Application Insights
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}
	}
}
