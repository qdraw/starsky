using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.Helpers;

[TestClass]
public sealed class GitShaAssemblyTest
{
	[TestMethod]
	[DataRow(false, null, "")]
	[DataRow(true, "   ", "")]
	[DataRow(true, "  abc123  ", "abc123")]
	public void GitHash_ShouldReturnExpectedValue(bool includeAttribute, string? gitCommitHash,
		string expected)
	{
		var assembly = CreateAssembly(includeAttribute, gitCommitHash);

		var result = GitShaAssembly.GitHash(assembly);

		Assert.AreEqual(expected, result);
	}

	private static AssemblyBuilder CreateAssembly(bool includeAttribute, string? gitCommitHash)
	{
		var assemblyName = new AssemblyName("GitShaAssemblyTest_" + Guid.NewGuid());
		var assemblyBuilder =
			AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

		if (!includeAttribute)
		{
			return assemblyBuilder;
		}

		var ctor = typeof(AssemblyMetadataAttribute).GetConstructor(
			[
				typeof(string),
				typeof(string)
			])!;
		var attribute = new CustomAttributeBuilder(ctor,
			[
				"GitCommitHash",
				gitCommitHash ?? string.Empty
			]);

		assemblyBuilder.SetCustomAttribute(attribute);

		return assemblyBuilder;
	}
}
