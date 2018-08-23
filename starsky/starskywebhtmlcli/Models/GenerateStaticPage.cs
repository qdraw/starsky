using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starskywebhtmlcli.Services;

namespace starskywebhtmlcli.Models
{
    // the sample base template class. It's not mandatory but I think it's much easier.
    public abstract class GenerateStaticPage
    {
        public List<FileIndexItem> Model;
        public AppSettings AppSettings;
        public string OutputFile;

        public void WriteLiteral(string literal)
        {
            // replace that by a text writer for example
            Console.Write(literal);
        }

        public void Write(object obj)
        {
            // replace that by a text writer for example
            Console.Write(obj);
            
            
//            if(OutputFile == null) throw new FileNotFoundException("outputFile missing");
//
//            var outputValue = (string) obj;
//            
//            new PlainTextFileHelper().WriteFile(OutputFile,outputValue);
        }

        public async virtual Task ExecuteAsync()
        {
            await Task.Yield(); // whatever, we just need something that compiles...
        }
    }
}