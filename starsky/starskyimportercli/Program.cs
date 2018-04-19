using System;

namespace starskyimportercli
{
    static class Program
    {
        static void Main(string[] args)
        {
            var inputFileFullPath = "";
            new ImportDatabase().ImportFile(inputFileFullPath);
            
        }
    }
}