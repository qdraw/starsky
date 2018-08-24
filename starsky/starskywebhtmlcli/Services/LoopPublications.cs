using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starsky.Models;
using starsky.Services;
using starskywebhtmlcli;

namespace starskywebhtmlcli.Services
{
    public class LoopPublications
    {
        private readonly AppSettings _appSettings;
        private readonly IServiceScopeFactory _startupHelper;

        public LoopPublications(AppSettings appSettings, IServiceScopeFactory startupHelper)
        {
            _startupHelper = startupHelper;
            _appSettings = appSettings;
        }
        
        public void Render(List<FileIndexItem> fileIndexItemsList)
        {
            if(!_appSettings.PublishProfiles.Any()) Console.WriteLine("There are no config items");
            
            foreach (var profile in _appSettings.PublishProfiles)
            {
                Console.WriteLine(profile.Path + " " +  profile.ContentType.ToString());

                if (profile.ContentType == TemplateContentType.Html)
                {
                        
                    var emailContent = Program.RenderViewAsync(_startupHelper,fileIndexItemsList).Result;
                    Console.WriteLine(emailContent);

                }
                
                if (profile.ContentType == TemplateContentType.Jpeg)
                {
                    foreach (var item in fileIndexItemsList)
                    {
                        new OverlayImage(_appSettings).ResizeOverlayImage(
                            _appSettings.DatabasePathToFilePath(item.FilePath), profile);
                    }
                }

                

            }
        }
    }
}