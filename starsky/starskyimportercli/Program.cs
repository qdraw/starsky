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

            
            var inputFileFullPath = "/data/isight/2018/2018-content/20180101_112936_imlo.jpg";
            new ImportDatabase().ImportFile(inputFileFullPath);
            
        }
    }
}