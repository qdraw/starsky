using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytests
{
    [TestClass]
    public class FileStreamingHelperTest
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task StreamFileExeption()
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
//            //Controller needs a controller context 
//            var controllerContext = new ControllerContext() {
//                HttpContext = httpContext,
//            };

            var ms = new MemoryStream();
            await FileStreamingHelper.StreamFile(httpContext.Request, ms);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task StreamFilemultipart()
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
            httpContext.Request.ContentType = "multipart/form-data";
            var ms = new MemoryStream();
            await FileStreamingHelper.StreamFile(httpContext.Request, ms);
        }

        [TestMethod]
        public void FileStreamingHelperTestGetEncodingTest()
        {
            
            var multipartSection = new MultipartSection();
            var bound = Encoding.UTF7.GetBytes("test");
            var boundMs = new MemoryStream();
            boundMs.Write(bound,0,bound.Length);
            var ms = new MemoryStream();
            ms.CopyTo(boundMs);
            multipartSection.Headers = new Dictionary<string, StringValues>();
            multipartSection.Headers.Add("Content-Type","text/plain");
            multipartSection.Body = ms;

            var result = FileStreamingHelper.GetEncoding(multipartSection);
            Assert.AreEqual(null,result);
            
        }

//        [TestMethod]
//        public async Task StreamFileBoundaryMultipart()
//        {
//            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
//            httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
//            httpContext.Request.ContentType = "multipart/form-data; boundary=--------------------------202544316310013789687781";
//
//            using (MemoryStream ms = new MemoryStream())
//            using (MemoryStream ms2 = new MemoryStream())
//
//            using (FileStream file = new FileStream(new CreateAnImage().FullFilePath, FileMode.Open, FileAccess.Read)) {
//                byte[] bytes = new byte[file.Length];
//                file.Read(bytes, 0, (int)file.Length);
//                ms.Write(bytes, 0, (int)file.Length);
//                
//                file.Dispose();
//
////                var bound = Encoding.ASCII.GetBytes("--------------------------202544316310013789687781");
////                var boundMs = new MemoryStream();
////                boundMs.Write(bound,0,bound.Length);
////                ms.CopyTo(boundMs);
//                
//                httpContext.Request.Body = ms;
//                await FileStreamingHelper.StreamFile(httpContext.Request, ms2);
//            }
           
 //       }
        
            
            
    }
}