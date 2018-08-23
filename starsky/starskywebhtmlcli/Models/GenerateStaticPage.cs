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
    public abstract class GenerateStaticPage : Microsoft.AspNetCore.Mvc.Razor.RazorPageBase 
    {
        public List<FileIndexItem> Model;
        public AppSettings AppSettings;
        public string OutputFile;
        
        public void WriteLiteral(string literal)
        {
            // used for header and footer
            if(OutputFile == null) throw new FileNotFoundException("outputFile missing");
            File.AppendAllText(OutputFile, literal);
        }

        public void Write(object obj)
        {
            // used for loop
            if(OutputFile == null) throw new FileNotFoundException("outputFile missing");
            string name = obj.ToString();
            name += Environment.NewLine;
            File.AppendAllText(OutputFile, name);
        }

        public override async Task ExecuteAsync()
        {
            await Task.Yield(); // whatever, we just need something that compiles...
        }
    }
}