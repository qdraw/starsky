using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers
{
	[TestClass]
	public class WindowsGetDefaultAppByExtensionTests
	{
		[TestMethod]
		public void WindowsGetDefaultAppByExtension1()
		{
			WindowsGetDefaultAppByExtension.GetDefaultApp1();

			WindowsGetDefaultAppByExtension.GetDefaultApp3();
		}
	}
}
