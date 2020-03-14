namespace starskycore.Models
{
    public class FolderOrFileModel
    {
	    /// <summary>
	    /// To Store output if file exist, folder or deleted
	    /// </summary>
        public FolderOrFileTypeList IsFolderOrFile { get; set; }
	    
	    /// <summary>
	    /// Enum FolderOrFileTypeList
	    /// </summary>
        public enum FolderOrFileTypeList
        {
            Folder = 1,
            File = 2,
            Deleted = 0
        }
    }
}
