using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.consoletelemetry.Initializers;

namespace starskytest.starsky.foundation.consoletelemetry.Initializers
{
	[TestClass]
	public class CloudRoleNameInitializerConsoleTest
	{
		[TestMethod]
		public void CloudRoleNameInitializerTestKeyword()
		{
			var telemetry = new EventTelemetry();
			new CloudRoleNameInitializer("test").Initialize(telemetry);
			
			Assert.AreEqual("test", telemetry.Context.Cloud.RoleName);
			Assert.AreEqual(Environment.MachineName, telemetry.Context.Cloud.RoleInstance);
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CloudRoleNameInitializerArgumentNullException()
		{
			new CloudRoleNameInitializer(null).Initialize(new EventTelemetry());
		}
				
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void CloudRoleNameInitializerArgumentNullException2()
		{
			new CloudRoleNameInitializer("test").Initialize(null);
		}
	}
}
