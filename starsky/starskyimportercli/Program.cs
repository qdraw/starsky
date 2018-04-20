using starsky.Models;
using starsky.Services;
using starskycli;

namespace starskyimportercli
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Check if user want more info
            AppSettingsProvider.Verbose = ArgsHelper.NeedVerbose(args);

            ConfigRead.SetAppSettingsProvider();

            
            var inputPath = "/data/isight/2018/01/test/";
            new ImportDatabase().Import(inputPath);
            
        }
    }
}