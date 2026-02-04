using System;

namespace starsky.foundation.platform.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class PackageTelemetryAttribute : Attribute
	{
		// Attribute is used to known if this property is used in the Telemetry
		// nothing here
	}
}
