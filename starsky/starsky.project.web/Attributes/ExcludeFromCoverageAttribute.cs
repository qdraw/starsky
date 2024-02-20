using System;

namespace starsky.project.web.Attributes
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
	public sealed class ExcludeFromCoverageAttribute : Attribute
	{
	}
}
