using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.Helpers
{
	public class DateAssemblyHealthCheck : IHealthCheck
	{
		/// <summary>
		/// Use as: .AddCheck<DateAssemblyHealthCheck>("DateAssemblyHealthCheck")
		/// </summary>
		/// <param name="context">HealthCheckContext</param>
		/// <param name="cancellationToken">to cancel item</param>
		/// <returns></returns>
		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var assemblyDate = GetBuildDate(Assembly.GetExecutingAssembly());

			return Task.FromResult(assemblyDate > DateTime.UtcNow ? HealthCheckResult.Unhealthy("Current Date is earlier then the Assembly is build") : 
				HealthCheckResult.Healthy("Current Date is after the Assembly is build :)"));
		}
		
		internal static DateTime GetBuildDate(Assembly assembly)
		{
			const string buildVersionMetadataPrefix = "+build";
			var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			if ( attribute?.InformationalVersion == null ) return new DateTime();
			var value = attribute.InformationalVersion;
			var index = value.IndexOf(buildVersionMetadataPrefix, StringComparison.Ordinal);
			if ( index <= 0 ) return new DateTime();
			value = value.Substring(index + buildVersionMetadataPrefix.Length);
			return DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, 
				DateTimeStyles.None, out var result) ? result : new DateTime();
		}
	}

}
