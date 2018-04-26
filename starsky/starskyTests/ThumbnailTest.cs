using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class ThumbnailTest
    {
        [TestMethod]
        public void CreateThumbTest()
        {

            var newImage = new CreateAnImage();
            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
            AppSettingsProvider.BasePath = newImage.BasePath;
            
            Thumbnail.CreateThumb(newImage.FullFilePath);
            
        }
    }
}