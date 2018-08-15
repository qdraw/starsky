using starsky.Models;

namespace starsky.Interfaces
{
    public interface IReadMeta
    {
        FileIndexItem ReadExifAndXmpFromFile(string singleFilePath);
//        FileIndexItem XmpGetSidecarFile(FileIndexItem databaseItem, string singleFilePath);
//        FileIndexItem ReadExifFromFile(string fileFullPath, FileIndexItem existingFileIndexItem = null);
    }
}