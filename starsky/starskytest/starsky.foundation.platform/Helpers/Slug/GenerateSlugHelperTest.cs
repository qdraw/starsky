using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers.Slug;

namespace starskytest.starsky.foundation.platform.Helpers.Slug;

[TestClass]
public class GenerateSlugHelperTest
{
	[TestMethod]
	public void AppSettingsGenerateSlugLengthCheck()
	{
		var slug = GenerateSlugHelper.GenerateSlug("1234567890123456789012345678901234567890123" +
		                                           "456789012345678901234567890123456789012345678901234567890" +
		                                           "456789012345678901234567890123456789012345678901234567890");
		Assert.AreEqual(GenerateSlugHelper.MaxLength, slug.Length);
	}

	[TestMethod]
	public void TestSpecialCharacters()
	{
		const string input = "Special #$ Characters";
		const string expectedOutput = "special-characters";
		var actualOutput = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	[TestMethod]
	public void TestLeadingTrailingSpaces()
	{
		const string input = "   Trim Spaces   ";
		const string expectedOutput = "trim-spaces";
		var actualOutput = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	[TestMethod]
	public void TestMixedCaseAndSpaces()
	{
		const string input = "Mixed Case   Slug";
		const string expectedOutput = "mixed-case-slug";
		var actualOutput = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	[TestMethod]
	public void TestTrailingHyphens()
	{
		const string input = "Trailing Hyphens -";
		const string expectedOutput = "trailing-hyphens";
		var actualOutput = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	[TestMethod]
	public void TestBeginHyphens()
	{
		const string input = " -Trailing Hyphens";
		const string expectedOutput = "trailing-hyphens";
		var actualOutput = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	[TestMethod]
	public void GenerateSlug_Lowercase_Disabled()
	{
		var slug = GenerateSlugHelper.GenerateSlug("ABC", true, false);
		Assert.AreEqual("ABC", slug);
	}

	[TestMethod]
	public void GenerateSlug_Lowercase_Enabled()
	{
		var slug = GenerateSlugHelper.GenerateSlug("ABC");
		Assert.AreEqual("abc", slug);
	}

	[TestMethod]
	public void GenerateSlug_AllowAt_DisabledByDefault()
	{
		var slug = GenerateSlugHelper.GenerateSlug("test@123");
		Assert.AreEqual("test123", slug);
	}

	[TestMethod]
	public void GenerateSlug_AllowAt_Enabled()
	{
		var slug = GenerateSlugHelper.GenerateSlug("test@123",
			false, true, true);
		Assert.AreEqual("test@123", slug);
	}

	[TestMethod]
	public void GenerateSlug_Trim()
	{
		var slug = GenerateSlugHelper.GenerateSlug("   abc   ");
		Assert.AreEqual("abc", slug);
	}

	[TestMethod]
	public void GenerateSlugTest_DashDashDash()
	{
		var slug = GenerateSlugHelper.GenerateSlug("test - test en test - test");
		Assert.AreEqual("test-test-en-test-test", slug);
	}

	[TestMethod]
	public void GenerateSlug_AllowUnderscore()
	{
		var slug = GenerateSlugHelper.GenerateSlug("a_b_c ", true);
		Assert.AreEqual("a_b_c", slug);
	}

	[TestMethod]
	public void GenerateSlug_Underscore_Disabled()
	{
		var slug = GenerateSlugHelper.GenerateSlug("a_b_c ");
		Assert.AreEqual("abc", slug);
	}

	[TestMethod]
	[DataRow("Café del Mar", "cafe-del-mar")]
	[DataRow("test - test en test - test", "test-test-en-test-test")]
	[DataRow("   abc   ", "abc")]
	[DataRow("a_b_c ", "abc")]
	[DataRow("test@123", "test123")]
	[DataRow("café", "cafe")]
	[DataRow("façade naïve", "facade-naive")]
	[DataRow("über-cool résumé", "uber-cool-resume")]
	[DataRow("simple", "simple")]
	[DataRow("", "")]
	[DataRow(null, null)]
	[DataRow("Łódź", "lodz")]
	[DataRow("Dvořák", "dvorak")]
	[DataRow("smörgåsbord", "smorgasbord")]
	[DataRow("coöperate", "cooperate")]
	[DataRow("123 élève", "123-eleve")]
	[DataRow("mañana", "manana")]
	[DataRow("Ærøskøbing", "aeroskobing")]
	[DataRow("piñata", "pinata")]
	[DataRow("Curaçao", "curacao")]
	public void GenerateSlug_ReturnsExpected(string input, string expected)
	{
		var result = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expected, result);
	}
}
