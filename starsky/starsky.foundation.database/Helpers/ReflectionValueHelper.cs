using System.Reflection;

namespace starsky.foundation.database.Helpers;

public static class ReflectionExtensions {
	public static T GetReflectionFieldValue<T>(this object obj, string name) {
		// Set the flags so that private and public fields from instances will be found
		const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		var field = obj.GetType().GetField(name, bindingFlags);
		return (T)field?.GetValue(obj);
	}
}

