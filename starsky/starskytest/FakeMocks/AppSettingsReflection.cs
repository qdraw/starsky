using System.Reflection;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public static class AppSettingsReflection
	{
		public static void Modify(AppSettings inputObject, string methodGetName = "get_DatabaseType", object value = null)
		{
			var type = typeof(AppSettings);
			foreach ( var property in type.GetProperties(BindingFlags.Public 
			                                             | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
				var getMethod = property.GetGetMethod(false);
				if (getMethod.GetBaseDefinition() == getMethod) {
					if ( methodGetName == getMethod.Name )
					{
						property.SetValue(inputObject, value, null);
					}
				}
			}
		}
	}
}
