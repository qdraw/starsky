using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace starskyiocli;

internal class ControllerMapper
{
	private readonly Dictionary<(string method, string? path), MethodInfo> _map = new();

	public ControllerMapper()
	{
		var controllers = Assembly.Load("starsky")
			.GetTypes()
			.Where(t => t.Name.EndsWith("Controller"));

		foreach ( var controller in controllers )
		{
			var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance);
			foreach ( var method in methods )
			{
				var getAttr = method.GetCustomAttribute<HttpGetAttribute>();
				if ( getAttr != null )
				{
					_map.Add(( "GET", getAttr.Template ), method);
				}

				var postAttr = method.GetCustomAttribute<HttpPostAttribute>();
				if ( postAttr != null )
				{
					_map.Add(( "POST", postAttr.Template ), method);
				}
			}
		}
	}


	/// <summary>
	///     Returns MethodInfo and controller Type for a given HTTP method + path
	/// </summary>
	private (MethodInfo methodInfo, Type controllerType) GetRoute(string reqMethod, string reqPath)
	{
		if ( !_map.TryGetValue(( reqMethod.ToUpper(), reqPath ), out var mi) )
		{
			throw new Exception($"No handler for {reqMethod} {reqPath}");
		}

		return ( mi, mi.DeclaringType! );
	}

	/// <summary>
	///     Invoke a controller method using DI
	/// </summary>
	public object? Invoke(string method, string path, Dictionary<string, string> parameters,
		IServiceProvider serviceProvider)
	{
		// Get MethodInfo and controller type
		var (mi, controllerType) = GetRoute(method, path);

		// Create controller instance with DI
		var controllerInstance = ActivatorUtilities.CreateInstance(serviceProvider, controllerType);

		// Map method parameters
		var args = mi.GetParameters()
			.Select(p => parameters.TryGetValue(p.Name, out var parameter)
				? Convert.ChangeType(parameter, p.ParameterType)
				: null)
			.ToArray();

		// Invoke the method
		return mi.Invoke(controllerInstance, args);
	}
}
