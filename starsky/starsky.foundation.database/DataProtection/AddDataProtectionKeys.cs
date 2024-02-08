using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.database.DataProtection;

public static class AddDataProtectionKeys
{
	public static void SetupDataProtection(this IServiceCollection services)
	{
		services.AddDataProtection()
			.AddKeyManagementOptions(options => options.XmlRepository =
				services.BuildServiceProvider().GetService<IXmlRepository>())
			.SetApplicationName("Starsky");
	}
}
