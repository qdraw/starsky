using System.Reflection;
using System.Threading.Tasks;

namespace starskytest.ExtensionMethods;

public static class InvokeReflection
{
	/// <summary>
	///     Invoke a method asynchronously
	/// </summary>
	/// <param name="this">The method to invoke</param>
	/// <param name="obj">The object to invoke the method on</param>
	/// <param name="parameters">The parameters to pass to the method</param>
	/// <returns>The result of the method</returns>
	public static async Task InvokeAsync(this MethodInfo @this, object obj,
		params object[] parameters)
	{
		dynamic awaitable = @this.Invoke(obj, parameters)!;
		await awaitable;
		awaitable.GetAwaiter();
	}
}
