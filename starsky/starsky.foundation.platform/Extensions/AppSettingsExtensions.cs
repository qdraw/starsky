using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.platform.Extensions
{
	public static class AppSettingsExtensions
	{
		public static TConfig ConfigurePoCo<TConfig>(
			this IServiceCollection services, IConfiguration configuration) where TConfig : class, new()
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(configuration);

			var config = new TConfig();
			configuration.Bind(config);
			services.AddSingleton(config);
			return config;
		}
	}
}
