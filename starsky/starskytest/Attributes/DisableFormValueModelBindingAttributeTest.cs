using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;

namespace starskytest.Attributes
{
	[TestClass]
	public sealed class DisableFormValueModelBindingAttributeTest
	{
		[TestMethod]
		public void DisableFormValueModelBindingAttribute_RemoveTest()
		{
			// source: https://stackoverflow.com/a/53141666/8613589

			var filter = new DisableFormValueModelBindingAttribute();
			var actionContext = new ActionContext(
				new DefaultHttpContext(),
				new RouteData(),
				new ActionDescriptor()
			);
			var filters = new List<IFilterMetadata>();
			var values = new List<IValueProviderFactory>();

			var context = new ResourceExecutingContext(actionContext, filters, values);

			//Run
			filter.OnResourceExecuting(context);

			// Run other class
			filter.OnResourceExecuted(new ResourceExecutedContext(actionContext, filters));

			// It is not removed in this case, because it didn't exist on forehand

			Assert.IsNotNull(filter);
			Assert.IsNotNull(actionContext);
			Assert.IsNotNull(filters);
		}
	}
}
