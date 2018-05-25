using System;
using System.Collections.Generic;
using System.Text;
using starsky.Services;

namespace starsky.Models
{
    // used for .config files
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
            set // using Json importer
            {
                if (string.IsNullOrEmpty(value) || value == "/") return;
                var structure = ConfigRead.PrefixBackslash(value);
                // todo: check if the feature works under windows
                _structure = ConfigRead.RemoveLatestBackslash(structure);
            }
        }

        public DateTime StructureDateTime;

        public List<int> StructureAsterixPositions()
        {
            return AllIndexesOf(_structure,"*");
        }
        
        private List<int> AllIndexesOf(string str, string value) {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0;; index += value.Length) {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        


//        public List<string> StructureFilenamePattern => _getFilenamePattern();
//        public List<string> StructureDirectoryPattern => _getFoldersPattern();
//
//        private List<string> _getFilenamePattern()
//        {
//            // 20180419_164921.exP
//            var structureAllSplit = AppSettingsProvider.Structure.Split("/");
//            if (structureAllSplit.Length <= 2) Console.WriteLine("Should be protected by Model");
//
//            return new List<string> {structureAllSplit[structureAllSplit.Length - 1]};
//        }
//
//        
//        private static List<string> _getFoldersPattern()
//        {
//            var structureAllSplit = AppSettingsProvider.Structure.Split("/");
//
//            if (structureAllSplit.Length <= 2)
//            {
//                var list = new List<string> {"/"};
//                return list;
//            }
//            // Return if nothing only a list with one slash
//
//
//            var structure = new List<string>();
//            for (int i = 1; i < structureAllSplit.Length-1; i++)
//            {
//                structure.Add(structureAllSplit[i]);
//            }
//            // else return the subfolders
//            return structure;
//        }
        
    }
}