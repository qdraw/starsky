using System;

namespace starskycore.Attributes
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
	public sealed class ExcludeFromCoverageAttribute : Attribute { }
}
