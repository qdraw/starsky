using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.ioc
{
	public class RegisterDependencies
	{
		public void Configure(IServiceCollection serviceCollection)
		{
			serviceCollection.AddClassesWithServiceAttribute("*");
		}
	}
}
