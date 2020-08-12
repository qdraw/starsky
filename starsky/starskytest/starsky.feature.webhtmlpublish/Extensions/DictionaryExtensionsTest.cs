using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Extensions;

namespace starskytest.starsky.feature.webhtmlpublish.Extensions
{
	[TestClass]
	public class DictionaryExtensionsTest
	{
		[TestMethod]
		public void AddRangeOverride()
		{
			var dictionary = new Dictionary<string,bool>
			{
				{
					"t",true
				}
			};
			
			dictionary.AddRangeOverride(new Dictionary<string, bool>
			{
				{
					"t",false
				}
			});
			
			Assert.IsFalse(dictionary["t"]);
		}

		[TestMethod]
		public void ForEach()
		{
			var dictionary = new Dictionary<string,bool>
			{
				{
					"t",true
				}
			};
			
			dictionary.ForEach(p =>
			{
				Assert.AreEqual("t",p.Key);
			});
		}
	}
}
