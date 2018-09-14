using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Net.Http.Headers;
using starsky.Models;
using starsky.Services;

namespace starsky.Helpers
{
    public static class FileStreamingHelper
    {
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public static async Task<List<string>> StreamFile(this HttpRequest request, AppSettings appSettings)
        {
            // The Header 'filename' is for uploading on file without a form.
            return await StreamFile(request.ContentType, request.Body, appSettings, HeaderFileName(request,appSettings));            
        }

        // Support for plain text input and base64 strings
        // Use for single files only
        public static string HeaderFileName(HttpRequest request, AppSettings appSettings)
        {
            // > when you do nothing
            if (string.IsNullOrEmpty(request.Headers["filename"]))
                return Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";
            
            // file without base64 encoding; return slug based url
            if (Base64Helper.TryParse(request.Headers["filename"]) == null)
                return appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(request.Headers["filename"]))
                       + Path.GetExtension(request.Headers["filename"]);
            
            var requestHeadersBytes = Base64Helper.TryParse(request.Headers["filename"]);
            var requestHeaders = System.Text.Encoding.ASCII.GetString(requestHeadersBytes);
            return appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(requestHeaders)) + Path.GetExtension(requestHeaders);
        }

        public static string DecodeFileName(string sourceFileName, AppSettings appSettings)
        {
            var byteName = Encoding.ASCII.GetBytes(appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(sourceFileName)));
            var tempHash = Base32.Encode(byteName) + "_00_" + Base32.Encode(FileHash.GenerateRandomBytes(5));
            return tempHash;
        }

        public static async Task<List<string>> StreamFile(string contentType, Stream requestBody, AppSettings appSettings, string headerFileName = null)
        {
            // headerFileName is for uploading on a single file without a multi part form.

            // fallback
            if (headerFileName == null) headerFileName = Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";
            
            var tempPaths = new List<string>();

            if (!MultipartRequestHelper.IsMultipartContentType(contentType))
            {
                if (contentType != "image/jpeg" && contentType != "application/octet-stream") 
                    throw new FileLoadException($"Expected a multipart request, but got {contentType}");

                var fullFilePath = Path.Combine(appSettings.TempFolder, headerFileName);
                
                // Write to disk
                await Store(fullFilePath, requestBody);

                tempPaths.Add(fullFilePath);

                return tempPaths;
            }
            
            // From here on no unit tests anymore :(
            
            // Used to accumulate all the form url encoded key value pairs in the 
            // request.

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(contentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, requestBody);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    var sourceFileName = contentDisposition.FileName.ToString().Replace("\"", string.Empty);
                    var inputExtension = Path.GetExtension(sourceFileName);

                    var tempHash = appSettings.GenerateSlug(Path.GetFileNameWithoutExtension(sourceFileName));
                    var fullFilePath = Path.Combine(appSettings.TempFolder, tempHash + inputExtension);
                    tempPaths.Add(fullFilePath);

                    await Store(fullFilePath, section.Body);
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return tempPaths;
        }

        private static async Task Store(string path, Stream stream)
        {
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream); // changed
            fileStream.Dispose();
        }

//        public static string GetTempFilePath(AppSettings appSettings, Stream stream )
//        {
//            var filename = FileHash.CalculateMd5AsyncWrapper(stream).Result;
//            return Path.Combine(appSettings.TempFolder, filename );
//        }

        
//        public static string GetTempFilePath(AppSettings appSettings, string baseFileName)
//        {
//            _appSettings = appSettings;
//            // Requires  AppSettingsProvider.ThumbnailTempFolder
//            // Requires: importIndexItem()
//            // Requires: AppSettingsProvider.Structure
//            
//            if (string.IsNullOrEmpty(baseFileName))
//            {
//                var guid = "_import_" + Guid.NewGuid().ToString().Substring(0, 5) + ".unknown";
//                guid = guid.Replace("=", string.Empty);
//                var path = Path.Combine(appSettings.TempFolder, guid);
//                return path;
//            }
//            
//            // // Escape a filename from nasty signs \/|:|\*|\?|\"|<|>|]
//            Regex illegalInFileName = new Regex("[\\/|:|\\*|\\?|\"|<|>|]");
//            baseFileName = illegalInFileName.Replace(baseFileName, string.Empty);
//
//            
//            var importIndexItem = new ImportIndexItem(_appSettings) {SourceFullFilePath = baseFileName};
//            
//            // Replace appendix with '-1' or '-222' ; (-22 will not be replaced)
//            // Assumes that a extension has always 3 letters. so no mp3 or html
//            importIndexItem.SourceFullFilePath = Regex.Replace(
//                importIndexItem.SourceFullFilePath, 
//                "\\-(\\d{3}|\\d)\\.\\w{3}$", 
//                importIndexItem.SourceFullFilePath.Substring(importIndexItem.SourceFullFilePath.Length - 4), 
//                RegexOptions.CultureInvariant);
//            
//            importIndexItem.ParseDateTimeFromFileName();
//            
//            
//            // Files that are not good parsed will be 00010101_000000.jpg
//            // By default those files are ignored by the ageing filter
//
//            var filename = importIndexItem.ParseFileName(false);
//            return Path.Combine(appSettings.TempFolder, filename );
//        }

        
//        // For reading plain text form fields
//                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
//                    {
//                        formAccumulator = await FormAccumulatorHelper(contentDisposition, section, formAccumulator);
//                    }
//        public static async Task<KeyValueAccumulator> FormAccumulatorHelper(ContentDispositionHeaderValue contentDisposition, 
//            MultipartSection section, KeyValueAccumulator formAccumulator)
//        {
//            // Content-Disposition: form-data; name="key"
//            // value
//
//            // Do not limit the key name length here because the 
//            // multipart headers length limit is already in effect.
//            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
//            var encoding = GetEncoding(section);
//            using (var streamReader = new StreamReader(
//                section.Body,
//                encoding,
//                detectEncodingFromByteOrderMarks: true,
//                bufferSize: 1024,
//                leaveOpen: true))
//            {
//                // The value length limit is enforced by MultipartBodyLengthLimit
//                var value = await streamReader.ReadToEndAsync();
//                if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
//                {
//                    value = String.Empty;
//                }
//                formAccumulator.Append(key.Value, value); // For .NET Core <2.0 remove ".Value" from key
//
//                if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
//                {
//                    throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
//                }
//            }
//            return formAccumulator;
//        }
//
//        public static Encoding GetEncoding(MultipartSection section)
//        {
//            MediaTypeHeaderValue mediaType;
//            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
//            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
//            // most cases.
//            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
//            {
//                return Encoding.UTF8;
//            }
//            return mediaType.Encoding;
//        }
    }
}
