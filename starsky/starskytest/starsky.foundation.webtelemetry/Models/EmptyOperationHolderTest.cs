using System.Reflection;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Models;

namespace starskytest.starsky.foundation.webtelemetry.Models
{
	[TestClass]
	public sealed class EmptyOperationHolderTest
	{
		[TestMethod]
		public void EmptyOperationHolder_Disposed()
		{
			var operationHolder = new EmptyOperationHolder<RequestTelemetry>();
			operationHolder.Dispose();
			
			var isDisposed = operationHolder.GetType().GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.NonPublic);
			
			var value = (bool?) isDisposed!.GetValue(operationHolder);
			Assert.IsTrue(value);
		}
	}
}
