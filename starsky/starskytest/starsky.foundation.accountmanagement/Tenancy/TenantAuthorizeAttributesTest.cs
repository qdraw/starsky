using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.foundation.accountmanagement.Middleware;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

[TestClass]
public sealed class TenantAuthorizeAttributesTest
{
	private static AuthorizationFilterContext Context(params Claim[] claims)
	{
		var httpContext = new DefaultHttpContext
		{
			User = claims.Length == 0
				? new ClaimsPrincipal(new ClaimsIdentity())
				: new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
		};
		return new AuthorizationFilterContext(
			new ActionContext(httpContext, new RouteData(), new ActionDescriptor()), []);
	}

	private static void AssertResult(AuthorizationFilterContext context, int status)
	{
		var result = context.Result as ContentResult;
		Assert.IsNotNull(result);
		Assert.AreEqual(status, result.StatusCode);
		// a response without a body is re-executed by the status code pages (a POST answers 405)
		Assert.IsFalse(string.IsNullOrEmpty(result.Content));
	}

	[TestMethod]
	public void TenantAdmin_Anonymous_401WithBody()
	{
		var context = Context();
		new TenantAdminAttribute().OnAuthorization(context);
		AssertResult(context, 401);
	}

	[TestMethod]
	public void TenantAdmin_User_403WithBody()
	{
		var context = Context(new Claim(ClaimTypes.Role, "User"));
		new TenantAdminAttribute().OnAuthorization(context);
		AssertResult(context, 403);
	}

	[TestMethod]
	public void TenantAdmin_Administrator_Passes()
	{
		var context = Context(new Claim(ClaimTypes.Role, "Administrator"));
		new TenantAdminAttribute().OnAuthorization(context);
		Assert.IsNull(context.Result);
	}

	[TestMethod]
	public void PlatformAdmin_Anonymous_401WithBody()
	{
		var context = Context();
		new PlatformAdminAttribute().OnAuthorization(context);
		AssertResult(context, 401);
	}

	[TestMethod]
	public void PlatformAdmin_TenantAdministrator_403WithBody()
	{
		var context = Context(new Claim(ClaimTypes.Role, "Administrator"));
		new PlatformAdminAttribute().OnAuthorization(context);
		AssertResult(context, 403);
	}

	[TestMethod]
	public void PlatformAdmin_Passes()
	{
		var context = Context(new Claim(TenantMembershipMiddleware.PlatformAdminClaim, "true"));
		new PlatformAdminAttribute().OnAuthorization(context);
		Assert.IsNull(context.Result);
	}
}
