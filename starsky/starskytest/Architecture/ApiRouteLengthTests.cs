using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;

namespace starskytest.Architecture;

[TestClass]
public class ApiRouteLengthTests
{
	/// <summary>
	/// Limit is for openapi.js and general usability
	/// </summary>
	private const int MaxRouteLength = 50;

	[TestMethod]
	public void All_HttpRoutes_In_All_Controllers_Should_Be_Shorter_Than_50_Characters()
	{
		// Arrange
		var assembly = typeof(AccountController).Assembly;

		var controllers = assembly
			.GetTypes()
			.Where(t =>
				typeof(ControllerBase).IsAssignableFrom(t) &&
				!t.IsAbstract);

		var violations = controllers
			.SelectMany(controller =>
			{
				var methods = controller
					.GetMethods(BindingFlags.Instance | BindingFlags.Public)
					.Where(m => !m.IsDefined(typeof(NonActionAttribute)));

				return methods.SelectMany(method =>
					method.GetCustomAttributes<HttpMethodAttribute>()
						.Where(attr => !string.IsNullOrWhiteSpace(attr.Template))
						.Select(attr => new
						{
							Controller = controller.Name,
							Method = method.Name,
							HttpVerb = string.Join(",", attr.HttpMethods),
							Route = attr.Template
						}));
			})
			.Where(x => x.Route?.Length > MaxRouteLength)
			.ToList();

		// Assert
		if ( violations.Count == 0 )
		{
			return;
		}

		var message = string.Join(Environment.NewLine,
			violations.Select(v =>
				$"{v.Controller}.{v.Method} [{v.HttpVerb}] -> {v.Route?.Length} chars: \"{v.Route}\""));

		Assert.Fail(
			$"One or more API routes exceed {MaxRouteLength} characters:{Environment.NewLine}{message}");
	}
}
