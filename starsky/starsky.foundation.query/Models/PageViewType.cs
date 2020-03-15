namespace starsky.foundation.query.Models
{
    public class PageViewType
    {
        public enum PageType
        {
            Archive = 1, // index
            DetailView = 2, 
            Search = 3, 
            Trash = 4,
            Unknown = -1
        }
    }
}
