using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.platform.Helpers
{
	public static class DateAssembly
	{
		/// <summary>
		/// Known when the build is done, uses UTC time
		/// </summary>
		/// <param name="assembly">the runtime assembly</param>
		/// <returns>Datetime or 0001:01:01</returns>
		public static DateTime GetBuildDate(Assembly assembly)
		{
			// Checks the PropertyGroup in the current Assembly/dll. So this foundation Assembly. Not the starsky assembly
			// <PropertyGroup>
			// <SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</SourceRevisionId>
			// </PropertyGroup>
			var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			if ( attribute?.InformationalVersion == null )
			{
				return new DateTime(0, DateTimeKind.Utc);
			}

			var value = attribute.InformationalVersion;
			return ParseBuildTime(value);
		}

		internal static DateTime ParseBuildTime(string value)
		{
			const string buildVersionMetadataPrefix = "+build";
			var index = value.IndexOf(buildVersionMetadataPrefix, StringComparison.Ordinal);
			if ( index <= 0 )
			{
				return new DateTime(0, DateTimeKind.Utc);
			}
			value = value.Substring(index + buildVersionMetadataPrefix.Length);
			return DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal, out var result) ?
					result :
					new DateTime(0, DateTimeKind.Utc);
		}
	}
}
