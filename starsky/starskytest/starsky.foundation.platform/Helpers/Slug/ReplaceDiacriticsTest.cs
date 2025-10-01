using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers.Slug;

namespace starskytest.starsky.foundation.platform.Helpers.Slug;

[TestClass]
public class ReplaceDiacriticsTest
{
	[TestMethod]
	[DataRow("café", "cafe")]
	[DataRow("façade naïve", "facade naive")]
	[DataRow("über-cool résumé", "uber-cool resume")]
	[DataRow("simple", "simple")]
	[DataRow("", "")]
	[DataRow(null, null)]
	[DataRow("Ærøskøbing", "Aeroskobing")]
	[DataRow("Łódź", "Lodz")]
	[DataRow("straße", "strasse")]
	[DataRow("Þingvellir", "Thingvellir")]
	[DataRow("Đakovo", "Djakovo")]
	[DataRow("Œuvre", "Oeuvre")]
	[DataRow("façade", "facade")]
	[DataRow("garçon", "garcon")]
	public void ReplaceText_VariousInputs_ReturnsExpected(string input, string expected)
	{
		var result = ReplaceDiacritics.ReplaceText(input);
		Assert.AreEqual(expected, result);
	}
}
