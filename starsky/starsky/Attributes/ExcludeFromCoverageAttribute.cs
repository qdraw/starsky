using System;

namespace starsky.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class ExcludeFromCoverageAttribute : Attribute { }
}