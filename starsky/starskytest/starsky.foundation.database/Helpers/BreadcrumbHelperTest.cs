using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.project.web.Attributes;

namespace starskytest.starsky.foundation.database.Helpers
{
	/// <summary>
	/// Also known as BreadcrumbsTest
	/// </summary>
	[TestClass]
	public sealed class BreadcrumbHelperTest
	{
		[TestMethod]
		[ExcludeFromCoverage]
		public void BreadcrumbSlashMethodTest()
		{
			var breadcrumbExample = Breadcrumbs.BreadcrumbHelper("/");

			var breadcrumblist = new List<string> { "/" };
			CollectionAssert.AreEqual(breadcrumbExample, breadcrumblist);
		}

		[TestMethod]
		public void BreadcrumbNoInputTest()
		{
			var breadcrumbExample = Breadcrumbs.BreadcrumbHelper(string.Empty);

			var breadcrumblist = new List<string> { "/" };
			CollectionAssert.AreEqual(breadcrumbExample, breadcrumblist);
		}

		[ExcludeFromCoverage]
		[TestMethod]
		public void BreadcrumbFileNameMethodTest()
		{
			var breadcrumbExample = Breadcrumbs.BreadcrumbHelper("/2018/2.jpg");
			var breadcrumblist = new List<string> { "/", "/2018" };
			CollectionAssert.AreEqual(breadcrumbExample, breadcrumblist);
		}

		[ExcludeFromCoverage]
		[TestMethod]
		public void BreadcrumbNullTest()
		{
			var breadcrumbExample = Breadcrumbs.BreadcrumbHelper(null);
			var breadcrumblist = new List<string>();
			CollectionAssert.AreEqual(breadcrumbExample, breadcrumblist);
		}

		[TestMethod]
		public void BreadcrumbHelperTest_WithoutStartSlash()
		{
			var breadcrumb = Breadcrumbs.BreadcrumbHelper("test");
			Assert.AreEqual("/", breadcrumb.FirstOrDefault());
		}

		[TestMethod]
		public void BreadcrumbHelperTest_ExampleBreadcrumb()
		{
			var breadcrumb = Breadcrumbs.BreadcrumbHelper("/test/file.jpg");
			Assert.AreEqual("/", breadcrumb.FirstOrDefault());
			Assert.AreEqual("/test", breadcrumb.LastOrDefault());
		}
	}
}
