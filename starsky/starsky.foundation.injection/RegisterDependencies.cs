using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.injection
{
	public class RegisterDependencies
	{
		/// <summary>
		/// Run through the entire solution and add Dependency injection
		/// Need to build afterwards
		/// </summary>
		/// <param name="serviceCollection">the ASP.Net service collection</param>
		public void Configure(IServiceCollection serviceCollection)
		{
			serviceCollection.AddClassesWithServiceAttribute("starsky*");
		}
	}
}
