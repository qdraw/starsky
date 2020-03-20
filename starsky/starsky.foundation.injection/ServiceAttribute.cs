using System;

namespace starsky.foundation.injection
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class ServiceAttribute : Attribute
	{
		public ServiceAttribute()
		{
			// nothing here
		}

		public ServiceAttribute(Type serviceType)
		{
			ServiceType = serviceType;
		}

		public InjectionLifetime InjectionLifetime { get; set; } = InjectionLifetime.Scoped;
		public Type ServiceType { get; set; }
	}
}
