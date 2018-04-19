using starsky.Services;

namespace starskyimportercli
{
    static class Program
    {
        static void Main(string[] args)
        {
            
            ConfigRead.SetAppSettingsProvider();

            
            var inputFileFullPath = "";
            new ImportDatabase().ImportFile(inputFileFullPath);
            
        }
    }
}