using System;

namespace starskycore.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class ExcludeFromCoverageAttribute : Attribute { }
}