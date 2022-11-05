using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Nuke.Common.Tooling;

namespace build
{
	[TypeConverter(typeof(TypeConverter<Configuration>))]
	[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
	[SuppressMessage("Usage", "S1104:Make this field 'private' and encapsulate it in a 'public' property")]
	[SuppressMessage("Usage", "S2223:Make this field 'private' and encapsulate it in a 'public' property")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public sealed class Configuration : Enumeration
	{
		public static Configuration Debug = new Configuration { Value = nameof(Debug) };
		public static Configuration Release = new Configuration { Value = nameof(Release) };
		public static implicit operator string(Configuration configuration)
		{
			return configuration.Value;
		}
	}
}

