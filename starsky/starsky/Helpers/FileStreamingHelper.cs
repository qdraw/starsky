using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using starsky.Models;

namespace starsky.Helpers
{
    public static class FileStreamingHelper
    {
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public static async Task<List<string>> StreamFile(this HttpRequest request)
        {
            return await StreamFile(request.ContentType, request.Body);            
        }

        public static async Task<List<string>> StreamFile(string contentType, Stream requestBody)
        {
            var tempPaths = new List<string>();

            if (!MultipartRequestHelper.IsMultipartContentType(contentType))
            {
                if (contentType != "image/jpeg")
                    throw new FileLoadException($"Expected a multipart request, but got {contentType}");
                
                var fullFilePath = GetTempFilePath();
                await Store(fullFilePath,requestBody);
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

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var fullFilePath = GetTempFilePath();
                        await Store(fullFilePath,section.Body);
                        tempPaths.Add(fullFilePath);
                    }
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
        
        public static string GetTempFilePath()
        {
            var guid = "_import_" + Guid.NewGuid().ToString().Substring(0, 20) + ".jpg";
            var path = Path.Combine(AppSettingsProvider.ThumbnailTempFolder, guid);
            return path;
        }

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
