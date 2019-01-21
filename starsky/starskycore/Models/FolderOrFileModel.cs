namespace starskycore.Models
{
    public class FolderOrFileModel
    {
        public FolderOrFileTypeList IsFolderOrFile { get; set; }
        public enum FolderOrFileTypeList
        {
            Folder = 1,
            File = 2,
            Deleted = 0
        }
    }
}
