using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;

namespace starskytests
{
    [TestClass]
    public class MigrationsTest
    {
        public MigrationsTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlite("Data Source=" + new CreateAnImage().BasePath + "temp.db");
            var options = builder.Options;
            _context = new ApplicationDbContext(options);
        }
        
        private readonly ApplicationDbContext _context;

        [TestMethod]
        public void MigrationsTest_contextDatabaseMigrate()
        {
            _context.Database.Migrate();
            File.Delete(new CreateAnImage().BasePath + "temp.db");
        }
    }
}