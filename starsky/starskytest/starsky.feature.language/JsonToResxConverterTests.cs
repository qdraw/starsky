using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.feature.language;

[TestClass]
public class JsonToResxConverterTests
{
	[TestMethod]
	public void TEst()
	{
		JsonToResxConverter.ConvertJsonToResx(
			"/Users/dion/data/git/starsky/starsky/starsky/clientapp/src/localization/localization.json",
			"/Users/dion/data/git/starsky/starsky/starsky.feature.language/LanguageContent.resx");
	}
}
