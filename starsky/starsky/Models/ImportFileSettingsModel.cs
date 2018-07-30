using System.ComponentModel.DataAnnotations;

namespace starsky.Models
{
    public class ImportSettingsModel
    {
        public ImportSettingsModel()
        {
            DeleteAfter = false;
            AgeFileFilter = true;
            RecursiveDirectory = false;
        }
        
        public bool DeleteAfter { get; set; }
        
        public bool AgeFileFilter { get; set; }
        
        public bool RecursiveDirectory { get; set; }

    }
}