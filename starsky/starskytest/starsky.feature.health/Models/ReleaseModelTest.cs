using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.UpdateCheck.Models;

namespace starskytest.starsky.feature.health.Models
{
	[TestClass]
	public class ReleaseModelTest
	{
		[TestMethod]
		public void TagNameNoNull()
		{
			var releaseModel = new ReleaseModel{TagName = null};
			Assert.AreEqual(string.Empty,releaseModel.TagName);
		}
	}
}
