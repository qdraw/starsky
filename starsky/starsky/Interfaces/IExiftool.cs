using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IExiftool
    {
        string BaseCommmand(string options, string fullFilePathSpaceSeperated);
//        void Update(FileIndexItem updateModel, List<string> inputFullFilePaths);
//        void Update(FileIndexItem updateModel, string inputFullFilePath);
        ExifToolModel Info(string fullFilePath);
    }
}