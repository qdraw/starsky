using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using starsky.foundation.platform.Interfaces;

namespace starsky.Helpers;

/// <summary>
///     A failed antiforgery check is a 400 without a body, which the status code pages
///     re-execute on the GET-only /error with the same method: a PUT or POST then answers
///     405. A body keeps the 400 and names the reason
/// </summary>
public sealed class AntiforgeryFailureFilter(IWebLogger logger) : IAlwaysRunResultFilter
{
	public void OnResultExecuting(ResultExecutingContext context)
	{
		if ( context.Result is not IAntiforgeryValidationFailedResult )
		{
			return;
		}

		logger.LogInformation(
			$"[AntiforgeryFailureFilter] token validation failed for {context.HttpContext.Request.Method} " +
			$"{context.HttpContext.Request.Path}; send a current X-XSRF-TOKEN header " +
			"(reload the page after login or a restart)");
		context.Result = new ContentResult
		{
			StatusCode = StatusCodes.Status400BadRequest,
			Content = "Antiforgery token validation failed"
		};
	}

	public void OnResultExecuted(ResultExecutedContext context)
	{
	}
}
