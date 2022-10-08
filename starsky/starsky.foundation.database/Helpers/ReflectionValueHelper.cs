using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace starsky.foundation.database.Helpers;

[SuppressMessage("Usage", "S3011:Make sure that this accessibility bypass is safe here", Justification = "Safe")]
public static class ReflectionExtensions {
	public static T GetReflectionFieldValue<T>(this object obj, string name) {
		// Set the flags so that private and public fields from instances will be found
		const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		var field = obj.GetType().GetField(name, bindingFlags);
		return ( T )field?.GetValue(obj)!;
	}
}

