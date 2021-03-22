using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace starsky.foundation.platform.Helpers
{
	public class DataProtectionService
	{
		public void Enable(IServiceCollection serviceCollection)
		{
			var sp = serviceCollection.BuildServiceProvider();
			
			serviceCollection.AddDataProtection()
				.AddKeyManagementOptions(options => options.XmlRepository = sp.GetService<IXmlRepository>());
		}
	}
}
