using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starskytest.FakeMocks;

namespace starskytest.Helpers;

[TestClass]
public sealed class AntiforgeryFailureFilterTest
{
	private sealed class FailedResult : IActionResult, IAntiforgeryValidationFailedResult
	{
		public System.Threading.Tasks.Task ExecuteResultAsync(ActionContext context)
		{
			return System.Threading.Tasks.Task.CompletedTask;
		}
	}

	private static ResultExecutingContext Context(IActionResult result)
	{
		var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(),
			new ActionDescriptor());
		return new ResultExecutingContext(actionContext, [], result, new object());
	}

	[TestMethod]
	public void AntiforgeryFailed_BecomesBadRequestWithBody()
	{
		var context = Context(new FailedResult());
		new AntiforgeryFailureFilter(new FakeIWebLogger()).OnResultExecuting(context);

		var result = context.Result as ContentResult;
		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
		Assert.IsFalse(string.IsNullOrEmpty(result.Content));
	}

	[TestMethod]
	public void OtherResult_Untouched()
	{
		var original = new OkResult();
		var context = Context(original);
		new AntiforgeryFailureFilter(new FakeIWebLogger()).OnResultExecuting(context);

		Assert.AreSame(original, context.Result);
	}
}
