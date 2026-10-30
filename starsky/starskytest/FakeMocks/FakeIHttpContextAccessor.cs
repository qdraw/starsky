using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace starskytest.FakeMocks;

/// <summary>
///     The real HttpContextAccessor keeps its context in a shared AsyncLocal,
///     so two instances in one test overwrite each other
/// </summary>
public sealed class FakeIHttpContextAccessor : IHttpContextAccessor
{
	public FakeIHttpContextAccessor(string? userName = "alice",
		string claimType = ClaimTypes.Name)
	{
		HttpContext = new DefaultHttpContext();
		if ( userName != null )
		{
			HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
				[new Claim(claimType, userName)], "test"));
		}
	}

	public HttpContext? HttpContext { get; set; }
}
