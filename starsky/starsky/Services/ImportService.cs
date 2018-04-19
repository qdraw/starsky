using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;
using starsky.Data;
using starsky.Models;

namespace starsky.Services
{
    public class ImportService
    {
        private readonly ApplicationDbContext _context;

        public ImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<string> ImportFile(string inputFileFullPath)
        {

            FileHash.GetHashCode(inputFileFullPath);
            
            var fileDateTime = DateTime.Now;
            
            var items = _getFoldersPattern();
            
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            
            _parseListDateFormat(items,fileDateTime);
            
            Console.WriteLine(items);

            return null;
        }

        private IEnumerable<string> _getFilenamePattern()
        {
            // 20180419_164921.exP
            var structureAllSplit = AppSettingsProvider.Structure.Split("/");
            if (structureAllSplit.Length <= 2) Console.WriteLine("Should be protected by Model");

            return new List<string> {structureAllSplit[structureAllSplit.Length - 1]};
        }

        
        private IEnumerable<string> _getFoldersPattern()
        {
            var structureAllSplit = AppSettingsProvider.Structure.Split("/");

            if (structureAllSplit.Length <= 2)
            {
                var list = new List<string> {"/"};
                return list;
            }
            // Return if nothing only a list with one slash


            var structure = new List<string>();
            for (int i = 1; i < structureAllSplit.Length-1; i++)
            {
                structure.Add(structureAllSplit[i]);
            }
            // else return the subfolders
            return structure;
        }

        private IEnumerable<string> _parseListDateFormat(IEnumerable<string> patternList, DateTime fileDateTime)
        {
            foreach (var patternItem in patternList)
            {
                var item = fileDateTime.ToString(patternItem, CultureInfo.InvariantCulture);
                Console.WriteLine(item);
            }
            return new List<string>();
        }
        
        // Add a new item to the database
        public ImportIndexItem AddItem(ImportIndexItem updateStatusContent)
        {
            if (!SqliteHelper.IsReady()) throw new ArgumentException("database error");
            
            try
            {
                _context.ImportIndex.Add(updateStatusContent);
                _context.SaveChanges();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(updateStatusContent);
                Console.WriteLine(e);
                throw;
            }

            return updateStatusContent;
        }
    }
    
    
}