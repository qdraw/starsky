using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class Sha256Test
{
	[TestMethod]
	public void Sha256_1()
	{
		var result = Sha256.ComputeSha256("test");
		Assert.AreEqual("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08",result);
	}
	
	[TestMethod]
	public void Sha256_2()
	{
		var result = Sha256.ComputeSha256(new byte[]{0,12,3,45,6,7,89,0});
		Assert.AreEqual("F2730FD7B558342F73A98810589BF8B9876BEE3FFBE79C873562CD88C4AD03FA",result);
	}
	
	[TestMethod]
	public void Sha256_Null_String()
	{
		var result = Sha256.ComputeSha256((null as string)!);
		Assert.AreEqual(string.Empty,result);
	}
	
	[TestMethod]
	public void Sha256_Null_ByteArray()
	{
		var result = Sha256.ComputeSha256(null as byte[]);
		Assert.AreEqual(string.Empty,result);
	}
	
	[TestMethod]
	public void Sha256_NoLenght_ByteArray()
	{
		var result = Sha256.ComputeSha256(Array.Empty<byte>());
		Assert.AreEqual(string.Empty,result);
	}
}
