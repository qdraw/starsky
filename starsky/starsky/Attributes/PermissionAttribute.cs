using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using starsky.foundation.accountmanagement.Services;

namespace starsky.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermissionAttribute(params UserManager.AppPermissions[] permissions)
	: AuthorizeAttribute, IAuthorizationFilter
{
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var user = context.HttpContext.User;

		if ( user.Identity?.IsAuthenticated == false )
		{
			context.Result = new UnauthorizedResult();
		}

		var collectedPermissions = new List<UserManager.AppPermissions>();
		foreach ( var permission in permissions )
		{
			var claim = user.Claims.FirstOrDefault(p =>
				p.Type == "Permission" && p.Value == permission.ToString());

			if ( claim == null )
			{
				continue;
			}

			collectedPermissions.Add(permission);
		}

		if ( collectedPermissions.Count == 0 )
		{
			context.Result = new UnauthorizedResult();
			return;
		}

		// add header for testing
		context.HttpContext.Response.Headers.TryAdd("x-permission", "true");
	}
}
