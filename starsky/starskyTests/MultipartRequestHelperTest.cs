using System.IO;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytests
{
    [TestClass]
    public class MultipartRequestHelperTest
    {

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void MultipartRequestHelperTest_Missingcontenttypeboundary()
        {
            var mediaType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("plain/text");
            MultipartRequestHelper.GetBoundary(mediaType, 50);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void MultipartRequestHelperTest_Multipartboundarylengthlimit()
        {
            var mediaType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("plain/text");
            mediaType.Boundary = new StringSegment("test");
            MultipartRequestHelper.GetBoundary(mediaType, 3);
        }

        [TestMethod]
        public void MultipartRequestHelperTest_boundarySucces()
        {
            var mediaType =
                new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("plain/text")
                {
                    Boundary = new StringSegment("test")
                };
            MultipartRequestHelper.GetBoundary(mediaType, 10);
        }

        [TestMethod]
        public void MultipartRequestHelperTest_IsMultipartContentType()
        {
            Assert.AreEqual(true,MultipartRequestHelper.IsMultipartContentType("multipart/"));
        }

        [TestMethod]
        public void MultipartRequestHelperTest_HasFormDataContentDisposition()
        {
            var formdata =
                new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("form-data");
            formdata.FileName = "test";
            formdata.FileNameStar = "1";
            Assert.AreEqual("form-data; filename=test; filename*=UTF-8''1",formdata.ToString());
            Assert.AreEqual(true,MultipartRequestHelper.HasFormDataContentDisposition(formdata));
        }
        
        [TestMethod]
        public void MultipartRequestHelperTest_HasFileContentDisposition()
        {
            var formdata =
                new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("form-data");
            formdata.FileName = "test";
            formdata.FileNameStar = "1";
            Assert.AreEqual("form-data; filename=test; filename*=UTF-8''1",formdata.ToString());
            Assert.AreEqual(true,MultipartRequestHelper.HasFileContentDisposition(formdata));

        }

    }
}