using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starskycore.Services;

namespace starskytest.Attributes
{
	[TestClass]
	public class PermissionAttributeTest
	{
		[TestMethod]
		public void NotLoggedIn()
		{
			var permissionAttribute = new PermissionAttribute(
				new List<UserManager.AppPermissions> {UserManager.AppPermissions.AppSettingsWrite}
					.ToArray());
			
			var authorizationFilterContext = new AuthorizationFilterContext(
				new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
				new List<IFilterMetadata>());
			
			permissionAttribute.OnAuthorization(authorizationFilterContext);
			
			Assert.AreEqual(authorizationFilterContext.Result.GetType(), new UnauthorizedResult().GetType());
		}
		
		[TestMethod]
		public void PermissionClaimMissing()
		{
			var permissionAttribute = new PermissionAttribute(
				new List<UserManager.AppPermissions> {UserManager.AppPermissions.AppSettingsWrite}
					.ToArray());

			var httpContext = new DefaultHttpContext
			{
				User = new ClaimsPrincipal(new ClaimsIdentity(
					new Claim[] {new Claim(ClaimTypes.Name, "username")}, "someAuthTypeName"))
			};
			
			var authorizationFilterContext = new AuthorizationFilterContext(
				new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
				new List<IFilterMetadata>());
			
			permissionAttribute.OnAuthorization(authorizationFilterContext);
			
			Assert.AreEqual(authorizationFilterContext.Result.GetType(), new UnauthorizedResult().GetType());
		}
		
		[TestMethod]
		public void PermissionClaimExist()
		{
			var permissionAttribute = new PermissionAttribute(
				new List<UserManager.AppPermissions> {UserManager.AppPermissions.AppSettingsWrite}
					.ToArray());

			var httpContext = new DefaultHttpContext
			{
				User = new ClaimsPrincipal(new ClaimsIdentity(
					new[] {new Claim("Permission", UserManager.AppPermissions.AppSettingsWrite.ToString())}))
			};
			
			var authorizationFilterContext = new AuthorizationFilterContext(
				new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
				new List<IFilterMetadata>());
			
			permissionAttribute.OnAuthorization(authorizationFilterContext);

			var existHeader = authorizationFilterContext.HttpContext.Response.Headers["x-permission"] == "true";
			Assert.IsTrue(existHeader);
		}
	}
}
