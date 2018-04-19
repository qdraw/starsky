using System.Collections.Generic;
using starsky.Services;

namespace starsky.Models
{
    public class BasePathConfig
    {
        private List<string> _readonly;
        public List<string> Readonly
        {
            get => _readonly;
            set
            {
                _readonly = new List<string>();

                if (value == null) return;

                foreach (var item in value)
                {
                    _readonly.Add(ConfigRead.RemoveLatestBackslash(item));
                }
            }
        }

        private string _structure;

        public string Structure
        {
            get
            {
                if (string.IsNullOrEmpty(_structure))
                {
                    // "dd" 	The day of the month, from 01 through 31.
                    // "MM" 	The month, from 01 through 12.
                    // "yyyy" 	The year as a four-digit number.
                    // "HH" 	The hour, using a 24-hour clock from 00 to 23.
                    // "mm" 	The minute, from 00 through 59.
                    // "ss" 	The second, from 00 through 59.
                    // --> / is split in folder
                    // ext = extension for example jpeg/jpg
                    
                    // future feature: (\w)? <= regex
                    // Currently only in strict mode
                    // So we dont accept '2011_01_01 vuurwerk'
                    // or 20180119_120000_DSC00009.jpg
                    
                    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
                    return "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
                }

                return _structure;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value == "/") return;
                var structure = ConfigRead.PrefixBackslash(value);
                _structure = ConfigRead.RemoveLatestBackslash(structure);
            }
        }
        
        
    }
}