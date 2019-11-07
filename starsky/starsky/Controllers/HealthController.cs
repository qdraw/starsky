using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.Controllers
{
	public class HealthController : Controller
	{
		/// <summary>
		/// Check if the installation is healthy (alpha, subject to change)
		/// </summary>
		/// <returns>status of the application</returns>
		/// <response code="200">success</response>
		[HttpGet("/api/health")]
		public IActionResult Health()
		{
			return Json(GetBuildDate(Assembly.GetExecutingAssembly()));
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
			return DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : new DateTime();
		}
	}
}
