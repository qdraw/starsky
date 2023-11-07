using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.http.Streaming
{
    public static class FileStreamingHelper
    {
        private static readonly FormOptions DefaultFormOptions = new FormOptions();
        
        /// <summary>
        /// Support for plain text input and base64 strings
        /// Use for single files only
        /// </summary>
        /// <param name="request">HttpRequest </param>
        /// <param name="appSettings">application settings</param>
        /// <returns></returns>
        public static string HeaderFileName(HttpRequest request, AppSettings appSettings)
        {
	        // > when you do nothing
	        if (string.IsNullOrEmpty(request.Headers["filename"]))
		        return Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";
            
	        // file without base64 encoding; return slug based url
	        if (Base64Helper.TryParse(request.Headers["filename"]).Length == 0)
		        return appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(request.Headers["filename"]),
			               true, false, true) + Path.GetExtension(request.Headers["filename"]);
            
	        var requestHeadersBytes = Base64Helper.TryParse(request.Headers["filename"]);
	        var requestHeaders = Encoding.ASCII.GetString(requestHeadersBytes);
	        return appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(requestHeaders),
		               true, false, true) + Path.GetExtension(requestHeaders);
        }
        
        public static async Task<List<string>> StreamFile(this HttpRequest request, 
	        AppSettings appSettings, ISelectorStorage selectorStorage)
        {
            // The Header 'filename' is for uploading on file without a form.
            return await StreamFile(request.ContentType, request.Body, 
	            appSettings, 
	            selectorStorage, HeaderFileName(request, appSettings));            
        }

        [SuppressMessage("Usage", "S125:Remove this commented out code")]
        [SuppressMessage("Usage", "S2589:contentDisposition null")]
        public static async Task<List<string>> StreamFile(string? contentType, Stream requestBody, AppSettings appSettings, 
	        ISelectorStorage selectorStorage, string? headerFileName = null)
        {
            // headerFileName is for uploading on a single file without a multi part form.

            // fallback
            headerFileName ??= Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";
            
            var tempPaths = new List<string>();

            if (!MultipartRequestHelper.IsMultipartContentType(contentType))
            {
                if (contentType != "image/jpeg" && contentType != "application/octet-stream") 
                    throw new FileLoadException($"Expected a multipart request, but got {contentType}; add the header 'content-type' ");

                var randomFolderName = "stream_" +
	                Base32.Encode(FileHash.GenerateRandomBytes(4));
                var fullFilePath = Path.Combine(appSettings.TempFolder, randomFolderName, headerFileName);

                // Write to disk
                var hostFileSystemStorage =
	                selectorStorage.Get(SelectorStorage.StorageServices
		                .HostFilesystem);
                hostFileSystemStorage.CreateDirectory(Path.Combine(appSettings.TempFolder, randomFolderName));
                await hostFileSystemStorage
	                .WriteStreamAsync(requestBody, fullFilePath);
                
                tempPaths.Add(fullFilePath);

                return tempPaths;
            }
            
            // Used to accumulate all the form url encoded key value pairs in the 
            // request.

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(contentType),
                DefaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, requestBody);

            var section = await reader.ReadNextSectionAsync();
            
            while (section != null)
            {
	            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
		            section.ContentDisposition, out var contentDisposition);
	            
	            if (hasContentDispositionHeader && contentDisposition != null && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    var sourceFileName = contentDisposition.FileName.ToString().Replace("\"", string.Empty);
                    var inputExtension = Path.GetExtension(sourceFileName).Replace("\n",string.Empty);

                    var tempHash = appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(sourceFileName),
	                    true, false, true); // underscore allowed
                    var randomFolderName = "stream_" +
                                           Base32.Encode(FileHash.GenerateRandomBytes(4));
                    var fullFilePath = Path.Combine(appSettings.TempFolder, randomFolderName, tempHash + inputExtension);
                    tempPaths.Add(fullFilePath);

                    var hostFileSystemStorage =
	                    selectorStorage.Get(SelectorStorage.StorageServices
		                    .HostFilesystem);
                    hostFileSystemStorage.CreateDirectory(Path.Combine(appSettings.TempFolder, randomFolderName));
                    
                    await hostFileSystemStorage
	                    .WriteStreamAsync(section.Body, fullFilePath);
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            
            return tempPaths;
        }
    }
}
