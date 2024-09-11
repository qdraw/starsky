using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.FtpAbstractions.Helpers;

namespace starskytest.starsky.feature.webftppublish.FtpAbstractionsTest;

[TestClass]
public sealed class WrapFtpWebRequestTest
{
	// Use abstraction
	// These are all null because WebRequest has no public ctor

	[TestMethod]
	public void Method_Set_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!) { Method = "t" });
	}

	[TestMethod]
	public void Method_Get_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() => new WrapFtpWebRequest(null!).Method);
	}

	[TestMethod]
	public void Credentials_Set_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!) { Credentials = new NetworkCredential() });
	}

	[TestMethod]
	public void Credentials_Get_Null()
	{
		// you are not allowed to get Credentials
		var result = new WrapFtpWebRequest(null!).Credentials;
		Assert.IsNull(result);
	}

	[TestMethod]
	public void UsePassive_Set_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!) { UsePassive = true });
	}

	[TestMethod]
	public void UsePassive_Get_Null()
	{
		Assert.ThrowsException<NullReferenceException>(
			() => new WrapFtpWebRequest(null!).UsePassive);
	}

	[TestMethod]
	public void UseBinary_Set_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!) { UseBinary = true });
	}

	[TestMethod]
	public void KeepAlive_Get_UseBinary()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!).UseBinary);
	}

	[TestMethod]
	public void KeepAlive_Set_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!) { KeepAlive = true });
	}

	[TestMethod]
	public void KeepAlive_Get_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			_ = new WrapFtpWebRequest(null!).KeepAlive);
	}

	[TestMethod]
	public void GetResponse_Get_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!).GetResponse());
	}

	[TestMethod]
	public void GetRequestStream_Get_Null()
	{
		Assert.ThrowsException<NullReferenceException>(() =>
			new WrapFtpWebRequest(null!).GetRequestStream());
	}
}
