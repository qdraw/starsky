using System;
using System.Collections.Generic;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskytest.Models
{
    public class FakeExiftool : IExiftool
    {
        public void Update(ExifToolModel updateModel, List<string> inputFullFilePaths)
        {
        }

        public void Update(ExifToolModel updateModel, string fullFilePath)
        {
        }

        public void Update(FileIndexItem updateModel, List<string> inputFullFilePaths)
        {
            throw new System.NotImplementedException();
        }

        public void Update(FileIndexItem updateModel, string inputFullFilePath)
        {
            throw new System.NotImplementedException();
        }

        public string BaseCommmand(string options, string fullFilePathSpaceSeperated)
        {
            Console.WriteLine(options);
            Console.WriteLine(fullFilePathSpaceSeperated);
            return string.Empty;
        }

        public ExifToolModel Info(string fullFilePath)
        {
            return new ExifToolModel();
        }
    }
}