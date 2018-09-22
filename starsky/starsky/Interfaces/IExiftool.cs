using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IExiftool
    {
        string BaseCommmand(string options, string fullFilePathSpaceSeperated);
        ExifToolModel Info(string fullFilePath);
    }
}