using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.Services;
using starskywebhtmlcli.Services;

namespace starskytests.starskyWebHtmlCli.Services
{
    [TestClass]
    public class LoopPublicationsTest
    {
        [TestMethod]
        public void LoopPublicationsTestRunVoid()
        {
            var appSettings = new AppSettings
            {
                PublishProfiles = new List<AppSettingsPublishProfiles>
                {
                    new AppSettingsPublishProfiles
                    {
                        ContentType = TemplateContentType.Html,
                        Path = "index.html",
                        Template = "Index.cshtml"
                    }
                }
            };
            var createAnImage = new CreateAnImage();
            
            appSettings.PublishProfiles.Add(new AppSettingsPublishProfiles
            {
                ContentType = TemplateContentType.Jpeg,
                Path = createAnImage.FullFilePath, 
                // Folder = ""
            });

            appSettings.StorageFolder = createAnImage.BasePath;
            appSettings.ThumbnailTempFolder = createAnImage.BasePath;
            var list = new List<FileIndexItem> {new FileIndexItem
            {
                FileName = createAnImage.FileName,
                FileHash = createAnImage.FileName.Replace(".jpg",string.Empty)
            }};

            new LoopPublications(appSettings).Render(list,null);
        }
    }
}