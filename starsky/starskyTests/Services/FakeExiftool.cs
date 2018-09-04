using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starskytests.Services
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

        public ExifToolModel Info(string fullFilePath)
        {
            return new ExifToolModel();
        }
    }
}