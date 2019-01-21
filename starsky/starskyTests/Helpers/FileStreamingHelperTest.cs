using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskytests.FakeCreateAn;

namespace starskytests.Helpers
{
    [TestClass]
    public class FileStreamingHelperTest
    {
        private readonly AppSettings _appSettings;

        public FileStreamingHelperTest()
        {
            // Add a dependency injection feature
            var services = new ServiceCollection();
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            var newImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", newImage.BasePath },
                { "App:ThumbnailTempFolder", newImage.BasePath },
                { "App:Verbose", "true" }
            };
            // Start using dependency injection
            var builder = new ConfigurationBuilder();  
            // Add random config to dependency injection
            builder.AddInMemoryCollection(dict);
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
        }
        
//        ContentDispositionHeaderValue.TryParse(
//        "form-data; name=\"file\"; filename=\"2017-12-07 17.01.25.png\"", out var contentDisposition);
//        var sectionSingle = new MultipartSection {Body = request.Body as MemoryStream};
//        sectionSingle.Headers = new Dictionary<string, StringValues>();
//        sectionSingle.Headers.Add("Content-Type",request.ContentType);
//        sectionSingle.Headers.Add("Content-Disposition","form-data; name=\"file2\"; filename=\"2017-12-07 17.01.25.png\"");

        [TestMethod]
        [ExpectedException(typeof(FileLoadException))]
        public async Task StreamFileExeption()
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
//            //Controller needs a controller context 
//            var controllerContext = new ControllerContext() {
//                HttpContext = httpContext,
//            };

            var ms = new MemoryStream();
            await FileStreamingHelper.StreamFile(httpContext.Request,_appSettings);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task StreamFilemultipart()
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
            httpContext.Request.ContentType = "multipart/form-data";
            var ms = new MemoryStream();
            await FileStreamingHelper.StreamFile(httpContext.Request,_appSettings);
        }

        [TestMethod]
        public async Task FileStreamingHelperTest_FileStreamingHelper_StreamFile_imagejpeg()
        {
            var targetStream = new MemoryStream();
            var createAnImage = new CreateAnImage();

            FileStream requestBody = new FileStream(createAnImage.FullFilePath, FileMode.Open);
            _appSettings.TempFolder = createAnImage.BasePath;
            
            var formValueProvider = await FileStreamingHelper.StreamFile("image/jpeg", requestBody, _appSettings);
            Assert.AreNotEqual(null, formValueProvider.ToString());
            requestBody.Dispose();
            
            // Clean
            File.Delete(formValueProvider.FirstOrDefault());
            
        }

//        [TestMethod]
//        public void FileStreamingHelper_GetTempFilePath_NullOption()
//        {
//            _appSettings.ThumbnailTempFolder = String.Empty;
//            var tempFilePath = FileStreamingHelper.GetTempFilePath(null,_appSettings);
//            Assert.AreEqual(true,tempFilePath.Contains("_import_"));
//        }
        
        
//        [TestMethod]
//        public void FileStreamingHelper_GetTempFilePath_ParseStringSimple_Option()
//        {
//            _appSettings.TempFolder = String.Empty;
//            _appSettings.Structure = "/yyyyMMdd_HHmmss.ext";
//
//            var tempFilePath = FileStreamingHelper.GetTempFilePath("20180123_132404.jpg",_appSettings);
//            Assert.AreEqual("20180123_132404.jpg",tempFilePath);
//        }
        
//        [TestMethod]
//        public void FileStreamingHelper_GetTempFilePath_ParseStringWithDots_Option()
//        {
//            _appSettings.TempFolder = string.Empty;
//            _appSettings.Structure = "/yyyyMMdd_HHmmss.ext";
//            var tempFilePath = FileStreamingHelper.GetTempFilePath("2018.01.23_13.24.04.jpg",_appSettings);
//            Assert.AreEqual("20180123_132404.jpg",tempFilePath);
//        }

        [TestMethod]
        public void FileStreamingHelper_HeaderFileName_normalStringTest()
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["filename"] = "2018-07-20 20.14.52.jpg"; //Set header
            var result = FileStreamingHelper.HeaderFileName(httpContext.Request,_appSettings);
            Assert.AreEqual("2018-07-20-201452.jpg",result);    
        }

        [TestMethod]
        public void FileStreamingHelper_HeaderFileName_base64StringTest()
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Headers["filename"] = "MjAxOC0wNy0yMCAyMC4xNC41Mi5qcGc="; //Set header
            var result = FileStreamingHelper.HeaderFileName(httpContext.Request,_appSettings);
            Assert.AreEqual("2018-07-20-201452.jpg",result);    
        }
        
//        [TestMethod]
//        public void FileStreamingHelper_GetTempFilePath_ParseStringAppendix1_Option()
//        {
//            _appSettings.TempFolder = string.Empty;
//            _appSettings.Structure = "/yyyyMMdd_HHmmss.ext";
//            var tempFilePath = FileStreamingHelper.GetTempFilePath("2018.01.23_13.24.04-1.jpg",_appSettings);
//            Assert.AreEqual("20180123_132404.jpg",tempFilePath);
//        }
        
      
        
        
        [TestMethod]
        public async Task FileStreamingHelperTest_FileStreamingHelper_StreamFile_multiPart()
        {
            var targetStream = new MemoryStream();
            var createAnImage = new CreateAnImage();
            
//            FileStream requestBody = new FileStream(createAnImage.FullFilePath, FileMode.Open);
            
//            var formValueProvider = await FileStreamingHelper.StreamFile("multipart/form-data; boundary=test", targetStream, targetStream);
//            Assert.AreNotEqual(null, formValueProvider.ToString());
//            requestBody.Dispose();
        }
        
        
//        [TestMethod]
//        public void Test()
//        {
//            ContentDispositionHeaderValue.TryParse(
//                "form-data; name=\"file\"; filename=\"2017-12-07 17.01.25.png\"", out var contentDisposition);
//
////            FileStreamingHelper.StreamFile("image/jpeg", Stream requestBody, Stream targetStream)
//             
//            
//            var sectionSingle = new MultipartSection {Body = request.Body as MemoryStream};
//            sectionSingle.Headers = new Dictionary<string, StringValues>();
//            sectionSingle.Headers.Add("Content-Type","image/jpeg");
//            sectionSingle.Headers.Add("Content-Disposition","form-data; name=\"file2\"; filename=\"2017-12-07 17.01.25.png\"");
//        }

//        [TestMethod]
//        public void FileStreamingHelperTestGetEncodingTest()
//        {
//            
//            var multipartSection = new MultipartSection();
//            var bound = Encoding.UTF7.GetBytes("test");
//            var boundMs = new MemoryStream();
//            boundMs.Write(bound,0,bound.Length);
//            var ms = new MemoryStream();
//            ms.CopyTo(boundMs);
//            multipartSection.Headers = new Dictionary<string, StringValues>();
//            multipartSection.Headers.Add("Content-Type","text/plain");
//            multipartSection.Body = ms;
//
//            var result = FileStreamingHelper.GetEncoding(multipartSection);
//            Assert.AreEqual(null,result);
//            
//        }

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
