using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using starskycore.Services;

namespace starsky.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class PermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
	{
		private readonly UserManager.PermissionEnum[] _permissions;
		public PermissionAttribute(params UserManager.PermissionEnum[] permission)
		{
			_permissions = permission;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result =  new ForbidResult();
			}

			var collectedPermissions = new List<UserManager.PermissionEnum>();
			foreach ( var permission in _permissions )
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
				context.Result =  new ForbidResult();
			}
		}
	}

}
