using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class GenerateSlugHelperTest
{
	[TestMethod]
	public void AppSettingsGenerateSlugLengthCheck()
	{
		var slug = GenerateSlugHelper.GenerateSlug("1234567890123456789012345678901234567890123" +
		                                           "456789012345678901234567890123456789012345678901234567890" +
		                                           "456789012345678901234567890123456789012345678901234567890");
		// Length == 65
		Assert.AreEqual(65, slug.Length);
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

	[DataTestMethod]
	[DataRow("", "")]
	[DataRow("Hello World", "hello-world")]
	[DataRow("Hello World!", "hello-world")]
	[DataRow("a_b_c ", "abc")]
	[DataRow("    a_b_c    ", "abc")]
	[DataRow("België", "belgie")]
	[DataRow("Café", "cafe")]
	[DataRow("Français", "francais")]
	[DataRow("München", "munchen")]
	[DataRow("São Paulo", "sao-paulo")]
	[DataRow("Niño", "nino")]
	[DataRow("ä", "a")]
	[DataRow("ë", "e")]
	[DataRow("é", "e")]
	[DataRow("ç", "c")]
	[DataRow("ü", "u")]
	[DataRow("ñ", "n")]
	[DataRow("ã", "a")]
	[DataRow("ô", "o")]
	[DataRow("ö", "o")]
	[DataRow("ß", "ss")]
	[DataRow("à", "a")]
	[DataRow("á", "a")]
	[DataRow("è", "e")]
	[DataRow("ê", "e")]
	[DataRow("ì", "i")]
	[DataRow("í", "i")]
	[DataRow("ò", "o")]
	[DataRow("ó", "o")]
	[DataRow("ù", "u")]
	[DataRow("ú", "u")]
	[DataRow("ý", "y")]
	[DataRow("ÿ", "y")]
	[DataRow("Æ", "ae")]
	[DataRow("æ", "ae")]
	[DataRow("Ø", "o")]
	[DataRow("ø", "o")]
	[DataRow("Å", "a")]
	[DataRow("å", "a")]
	[DataRow("ł", "l")]
	[DataRow("ž", "z")]
	[DataRow("š", "s")]
	[DataRow("č", "c")]
	[DataRow("đ", "d")]
	[DataRow("ğ", "g")]
	[DataRow("ı", "i")]
	[DataRow("ń", "n")]
	[DataRow("ř", "r")]
	[DataRow("ą", "a")]
	[DataRow("ę", "e")]
	[DataRow("œ", "oe")]
	[DataRow("þ", "th")]
	[DataRow("ð", "d")]
	[DataRow("ħ", "h")]
	[DataRow("©", "c")]
	[DataRow("®", "r")]
	[DataRow("™", "tm")]
	public void GenerateSlug(string input, string expectedOutput)
	{
		// underscore is disabled by default
		var slug = GenerateSlugHelper.GenerateSlug(input);
		Assert.AreEqual(expectedOutput, slug);
	}
}
