using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;

namespace starskytest.AuthorizeAttributeTest;

[TestClass]
public class TestForNonAuthorizeAttributesTest
{
	private static Type[] GetControllersInNamespace(Assembly assembly, string controllerNamespace)
	{
		return assembly.GetTypes().Where(types =>
				string.Equals(types.Namespace, controllerNamespace, StringComparison.Ordinal))
			.ToArray();
	}

	private static (Assembly, string) GetAssembly<T>()
	{
		var getEverythingBeforeLastDotRegex = new Regex(".*(?=\\.)", RegexOptions.None,
			TimeSpan.FromMilliseconds(200));

		var fullName = typeof(T).FullName;
		if ( string.IsNullOrEmpty(fullName) )
		{
			throw new Exception("Type does not have a FullName");
		}

		var name = getEverythingBeforeLastDotRegex.Match(fullName).Value;
		var assembly = typeof(T).Assembly;
		return ( assembly, name );
	}

	[SuppressMessage("Usage",
		"S6602:\"Find\" method should be used instead of the \"FirstOrDefault\" extension")]
	private static List<MethodInfo> GetControllerMethods(Type projectController)
	{
		var projectMethods =
			projectController.GetMethods(BindingFlags.Public | BindingFlags.Instance);

		var (controllerBaseAssembly, controllerBaseName) = GetAssembly<ControllerBase>();
		var controllersBaseInNamespace =
			GetControllersInNamespace(controllerBaseAssembly, controllerBaseName);
		var controllersBaseInNamespaceMethods =
			controllersBaseInNamespace.SelectMany(p => p.GetMethods()).ToArray();

		var (controllerGenericTypeAssembly, controllerGenericTypeName) = GetAssembly<Controller>();
		var controllersGenericTypeInNamespace =
			GetControllersInNamespace(controllerGenericTypeAssembly, controllerGenericTypeName);
		var controllersGenericTypeNamespaceMethods = controllersGenericTypeInNamespace
			.SelectMany(p => p.GetMethods()).ToArray();

		var allGenericControllerTypes = controllersBaseInNamespaceMethods
			.Concat(controllersGenericTypeNamespaceMethods).ToArray();

		var projectsThatNotInheritFromControllerBase = new List<MethodInfo>();
		// ReSharper disable once LoopCanBeConvertedToQuery
		foreach ( var projectMethod in projectMethods )
		{
			var match = allGenericControllerTypes.FirstOrDefault(p => p.Name == projectMethod.Name);
			if ( match == null )
			{
				projectsThatNotInheritFromControllerBase.Add(projectMethod);
			}
		}

		return projectsThatNotInheritFromControllerBase;
	}

	[TestMethod]
	public void TestForNonAuthorizeAttributes()
	{
		var (assembly, name) = GetAssembly<HomeController>();
		var controllersInNamespace = GetControllersInNamespace(assembly, name);

		foreach ( var controller in controllersInNamespace )
		{
			var methods = GetControllerMethods(controller);
			foreach ( var method in methods )
			{
				var authorizeAttributes =
					method.GetCustomAttributes(typeof(AuthorizeAttribute), true);
				var authorizeParentAttributes =
					method.DeclaringType?.GetCustomAttributes(typeof(AuthorizeAttribute), true) ??
					Array.Empty<object>();
				var allowAnonymousAttributes =
					method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);
				var allowAnonymousParentAttributes =
					method.DeclaringType?.GetCustomAttributes(typeof(AllowAnonymousAttribute),
						true) ?? Array.Empty<object>();

				var attributes = new List<object>();
				attributes.AddRange(authorizeAttributes);
				attributes.AddRange(authorizeParentAttributes);
				attributes.AddRange(allowAnonymousAttributes);
				attributes.AddRange(allowAnonymousParentAttributes);

				Assert.IsTrue(attributes.Count != 0,
					$"No AuthorizeAttribute found on {controller.FullName} {method.Name} method");
			}
		}
	}
}
