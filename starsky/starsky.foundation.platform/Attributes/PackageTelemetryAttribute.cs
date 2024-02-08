using System;
using System.Diagnostics.CodeAnalysis;

namespace starsky.foundation.platform.Attributes
{
	[SuppressMessage("Design", "CA1018:Mark attributes with AttributeUsageAttribute")]
	public sealed class PackageTelemetryAttribute : Attribute
	{
		// Attribute is used to known if this property is used in the Telemetry
		// nothing here
	}
}

