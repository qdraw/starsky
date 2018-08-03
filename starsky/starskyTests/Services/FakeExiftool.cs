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
        public ExifToolModel Update(ExifToolModel updateModel, List<string> inputFullFilePaths)
        {
            return updateModel;
        }

        public ExifToolModel Update(ExifToolModel updateModel, string fullFilePath)
        {
            return updateModel;
        }

        public ExifToolModel Info(string fullFilePath)
        {
            return new ExifToolModel();
        }
    }
}