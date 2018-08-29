using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;
using starskywebhtmlcli;
using starskywebhtmlcli.ViewModels;

namespace starskywebhtmlcli.Services
{
    public class LoopPublications
    {
        private readonly AppSettings _appSettings;

        public LoopPublications(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        public void Render(List<FileIndexItem> fileIndexItemsList, string[] base64ImageArray)
        {
            if(!_appSettings.PublishProfiles.Any()) Console.WriteLine("There are no config items");
            
            foreach (var profile in _appSettings.PublishProfiles)
            {
                Console.WriteLine(profile.Path + " " +  profile.ContentType.ToString());

                if (profile.ContentType == TemplateContentType.Html)
                {
                    var viewModel = new WebHtmlViewModel
                    {
                        AppSettings = _appSettings,
                        Profile = profile,
                        Base64ImageArray = base64ImageArray,
                        // apply slug to items, but use it only in the copy
                        FileIndexItems = fileIndexItemsList.Select(c => c.Clone()).ToList(),
                    };

                    // add to IClonable
                    foreach (var item in viewModel.FileIndexItems)
                    {
                        item.FileName = _appSettings.GenerateSlug(item.FileCollectionName) + Path.GetExtension(item.FileName);
                    }

//                    Files.DeleteFile(profile.Path);
                    
                    var embeddedResult = new ParseRazor().EmbeddedViews(profile.Template,viewModel).Result;
                    new PlainTextFileHelper().WriteFile(_appSettings.StorageFolder 
                                                        + profile.Path, embeddedResult);
                    Console.WriteLine(embeddedResult);
                }
                
                if (profile.ContentType == TemplateContentType.Jpeg)
                {
                    foreach (var item in fileIndexItemsList)
                    {
                        var overlayImage = new OverlayImage(_appSettings);

                        var fullFilePath = _appSettings.DatabasePathToFilePath(item.FilePath);

                        var outputFilePath = overlayImage.FilePathOverlayImage(fullFilePath, profile);
                        
                        // check if subfolder '1000' exist on disk
                        var toCreateSubfolder = _appSettings.DatabasePathToFilePath(profile.Folder, false);

                        if (Files.IsFolderOrFile(toCreateSubfolder) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
                        {
                            Directory.CreateDirectory(toCreateSubfolder);
                        }
                        
                        // for less than 1000px
                        if (profile.SourceMaxWidth <= 1000)
                        {
                            var inputFullFilePath = new Thumbnail(_appSettings).GetThumbnailPath(item.FileHash);
                            new OverlayImage(_appSettings).ResizeOverlayImage(
                                inputFullFilePath, outputFilePath,profile);
                        }
                            
                        // Thumbs are 1000 px
                        if (profile.SourceMaxWidth > 1000)
                        {
                            overlayImage.ResizeOverlayImage(fullFilePath, outputFilePath, profile);
                        }
                        

                    }
                }

                

            }
        }
    }
}