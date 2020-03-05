using System;

namespace starsky.foundation.ioc
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class ServiceAttribute : Attribute
	{
		public ServiceAttribute()
		{

		}

		public ServiceAttribute(Type serviceType)
		{
			ServiceType = serviceType;
		}

		public InjectionLifetime InjectionLifetime { get; set; } = InjectionLifetime.Scoped;
		public Type ServiceType { get; set; }
	}
}
