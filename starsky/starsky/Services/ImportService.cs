using System;
using System.Collections.Generic;
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
            Console.WriteLine("Hello!");
            return null;
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