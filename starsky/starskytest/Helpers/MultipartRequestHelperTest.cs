using System.IO;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Streaming;

namespace starskytest.Helpers;

[TestClass]
public sealed class MultipartRequestHelperTest
{
	[TestMethod]
	public void MultipartRequestHelperTest_MissingContentTypeBoundary_InvalidDataException()
	{
		var mediaType = new MediaTypeHeaderValue("plain/text");

		Assert.ThrowsException<InvalidDataException>(() =>
			MultipartRequestHelper.GetBoundary(mediaType, 50));
	}

	[TestMethod]
	public void MultipartRequestHelperTest_MultipartBoundaryLengthLimit_InvalidDataException()
	{
		var mediaType =
			new MediaTypeHeaderValue("plain/text") { Boundary = new StringSegment("test") };
		Assert.ThrowsException<InvalidDataException>(() =>
			MultipartRequestHelper.GetBoundary(mediaType, 3));
	}

	[TestMethod]
	public void MultipartRequestHelperTest_boundarySuccess()
	{
		var mediaType =
			new MediaTypeHeaderValue("plain/text") { Boundary = new StringSegment("test") };
		var boundary = MultipartRequestHelper.GetBoundary(mediaType, 10);
		Assert.AreEqual("test", boundary);
	}

	[TestMethod]
	public void MultipartRequestHelperTest_IsMultipartContentType()
	{
		Assert.IsTrue(MultipartRequestHelper.IsMultipartContentType("multipart/"));
	}

	[TestMethod]
	public void MultipartRequestHelperTest_HasFormDataContentDispositionFalse()
	{
		var formData =
			new ContentDispositionHeaderValue("form-data")
			{
				FileName = "test", FileNameStar = "1"
			};
		Assert.AreEqual("form-data; filename=test; filename*=UTF-8''1", formData.ToString());
		Assert.IsFalse(MultipartRequestHelper.HasFormDataContentDisposition(formData));
	}

	[TestMethod]
	public void MultipartRequestHelperTest_HasFormDataContentDispositionTrue()
	{
		var formData =
			new ContentDispositionHeaderValue("form-data");
		Assert.IsTrue(MultipartRequestHelper.HasFormDataContentDisposition(formData));
	}

	[TestMethod]
	public void MultipartRequestHelperTest_HasFileContentDisposition()
	{
		var formData =
			new ContentDispositionHeaderValue("form-data")
			{
				FileName = "test", FileNameStar = "1"
			};
		Assert.AreEqual("form-data; filename=test; filename*=UTF-8''1", formData.ToString());
		Assert.IsTrue(MultipartRequestHelper.HasFileContentDisposition(formData));
	}
}
