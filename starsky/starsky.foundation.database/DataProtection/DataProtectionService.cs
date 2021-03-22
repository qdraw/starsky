using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.database.DataProtection
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
