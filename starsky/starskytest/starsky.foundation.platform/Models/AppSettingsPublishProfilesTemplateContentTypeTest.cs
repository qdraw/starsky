using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsPublishProfilesTemplateContentTypeTest
{
	[TestMethod]
	public void GetAll_ExcludesNoneAndReturnsAllOtherTypes()
	{
		// Act
		var result = AppSettingsPublishProfilesTemplateContentType
			.GetAll().Select(p => p.Type);

		// Assert
		Assert.IsNotNull(result);
		var list = result.ToList();
		Assert.DoesNotContain(TemplateContentType.None, list, "Should not contain None");
		// Should contain all other enum values
		var allEnumValues = Enum.GetValues(typeof(TemplateContentType))
			.Cast<TemplateContentType>()
			.Where(e => e != TemplateContentType.None)
			.ToList();
		CollectionAssert.AreEquivalent(allEnumValues, list);
	}
}
